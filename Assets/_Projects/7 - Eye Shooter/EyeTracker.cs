using UnityEngine;
using OpenCvSharp;
using System.Collections.Generic;
using System.Linq;

namespace EyeShooter
{
    /// <summary>
    /// Handles eye detection and tracking using OpenCV.
    /// Detects face, eyes, pupils, and blinks from webcam input.
    /// </summary>
    public class EyeTracker : MonoBehaviour
    {
        /// <summary>
        /// Minimum eye aspect ratio threshold for open eye detection
        /// </summary>
        private const float BLINK_THRESHOLD = 0.7f;

        /// <summary>
        /// Number of consecutive frames required to confirm a blink
        /// </summary>
        private const int BLINK_FRAME_COUNT = 2;

        /// <summary>
        /// Cooldown time between blinks in seconds
        /// </summary>
        private const float BLINK_COOLDOWN = 0.3f;

        /// <summary>
        /// Scale factor for image pyramid in face detection
        /// </summary>
        private const double FACE_SCALE_FACTOR = 1.1;

        /// <summary>
        /// Minimum number of neighbor rectangles for face detection
        /// </summary>
        private const int FACE_MIN_NEIGHBORS = 5;

        /// <summary>
        /// Smoothing factor for eye position (0-1, higher = more smoothing)
        /// </summary>
        private const float POSITION_SMOOTHING = 0.7f;

        [Header("Camera Settings")]
        [SerializeField]
        [Tooltip("Index of the webcam to use (0 = default)")]
        private int cameraIndex = 0;

        [SerializeField]
        [Tooltip("Target frame rate for webcam capture")]
        private int targetFPS = 30;

        [Header("Detection Settings")]
        [SerializeField]
        [Tooltip("Minimum size for face detection")]
        private Vector2Int minFaceSize = new Vector2Int(100, 100);

        [SerializeField]
        [Tooltip("Enable debug visualization")]
        private bool showDebugWindow = false;

        /// <summary>
        /// OpenCV video capture object for webcam input
        /// </summary>
        private VideoCapture videoCapture;

        /// <summary>
        /// Haar cascade classifier for face detection
        /// </summary>
        private CascadeClassifier faceCascade;

        /// <summary>
        /// Haar cascade classifier for eye detection
        /// </summary>
        private CascadeClassifier eyeCascade;

        /// <summary>
        /// Current frame from webcam
        /// </summary>
        private Mat currentFrame;

        /// <summary>
        /// Grayscale version of current frame for processing
        /// </summary>
        private Mat grayFrame;

        /// <summary>
        /// Detected face region in current frame
        /// </summary>
        private OpenCvSharp.Rect faceRegion;

        /// <summary>
        /// Normalized pupil position (0-1 range for both x and y)
        /// </summary>
        private Vector2 normalizedPupilPosition;

        /// <summary>
        /// Smoothed pupil position for stable crosshair movement
        /// </summary>
        private Vector2 smoothedPosition;

        /// <summary>
        /// Counter for consecutive blink frames
        /// </summary>
        private int blinkFrameCounter;

        /// <summary>
        /// Flag indicating if a blink was detected this frame
        /// </summary>
        private bool blinkDetectedThisFrame;

        /// <summary>
        /// Timestamp of last blink detection
        /// </summary>
        private float lastBlinkTime;

        /// <summary>
        /// Initializes OpenCV components and starts webcam capture
        /// </summary>
        private void Start()
        {
            InitializeOpenCV();
            InitializeCamera();
        }

        /// <summary>
        /// Sets up OpenCV cascade classifiers for face and eye detection
        /// </summary>
        private void InitializeOpenCV()
        {
            string haarCascadePath = System.IO.Path.Combine(
                Application.streamingAssetsPath, 
                "haarcascades"
            );

            faceCascade = new CascadeClassifier(
                System.IO.Path.Combine(haarCascadePath, "haarcascade_frontalface_default.xml")
            );

            eyeCascade = new CascadeClassifier(
                System.IO.Path.Combine(haarCascadePath, "haarcascade_eye.xml")
            );

            currentFrame = new Mat();
            grayFrame = new Mat();
        }

        /// <summary>
        /// Initializes and starts the webcam capture
        /// </summary>
        private void InitializeCamera()
        {
            videoCapture = new VideoCapture(cameraIndex);
            videoCapture.Set(VideoCaptureProperties.Fps, targetFPS);

            if (!videoCapture.IsOpened())
            {
                Debug.LogError($"Failed to open camera {cameraIndex}");
            }
        }

        /// <summary>
        /// Processes each frame from webcam and updates eye tracking data
        /// </summary>
        private void Update()
        {
            if (!videoCapture.IsOpened()) return;

            CaptureFrame();
            ProcessFrame();
            
            if (showDebugWindow)
            {
                ShowDebugVisualization();
            }
        }

        /// <summary>
        /// Captures a new frame from the webcam
        /// </summary>
        private void CaptureFrame()
        {
            videoCapture.Read(currentFrame);
            
            if (currentFrame.Empty()) return;

            Cv2.CvtColor(currentFrame, grayFrame, ColorConversionCodes.BGR2GRAY);
            Cv2.EqualizeHist(grayFrame, grayFrame);
        }

        /// <summary>
        /// Processes the current frame to detect face, eyes, and pupils
        /// </summary>
        private void ProcessFrame()
        {
            blinkDetectedThisFrame = false;

            if (!DetectFace()) return;

            List<OpenCvSharp.Rect> eyes = DetectEyes();
            
            if (eyes.Count >= 2)
            {
                ProcessEyeRegions(eyes);
            }
        }

        /// <summary>
        /// Detects face in the current frame
        /// </summary>
        /// <returns>True if a face was detected</returns>
        private bool DetectFace()
        {
            OpenCvSharp.Rect[] faces = faceCascade.DetectMultiScale(
                grayFrame,
                FACE_SCALE_FACTOR,
                FACE_MIN_NEIGHBORS,
                HaarDetectionTypes.ScaleImage,
                new Size(minFaceSize.x, minFaceSize.y)
            );

            if (faces.Length == 0) return false;

            faceRegion = faces[0];
            return true;
        }

        /// <summary>
        /// Detects eyes within the face region
        /// </summary>
        /// <returns>List of detected eye rectangles</returns>
        private List<OpenCvSharp.Rect> DetectEyes()
        {
            Mat faceROI = new Mat(grayFrame, faceRegion);

            OpenCvSharp.Rect[] detectedEyes = eyeCascade.DetectMultiScale(
                faceROI,
                FACE_SCALE_FACTOR,
                FACE_MIN_NEIGHBORS,
                HaarDetectionTypes.ScaleImage,
                new Size(30, 30)
            );

            return detectedEyes
                .OrderBy(e => e.X)
                .Take(2)
                .ToList();
        }

        /// <summary>
        /// Processes detected eye regions to extract pupil position and detect blinks
        /// </summary>
        /// <param name="eyes">List of detected eye rectangles</param>
        private void ProcessEyeRegions(List<OpenCvSharp.Rect> eyes)
        {
            OpenCvSharp.Rect rightEye = eyes[0];
            Vector2 pupilPos = DetectPupil(rightEye);
            
            UpdateNormalizedPosition(pupilPos, rightEye);
            SmoothPosition();
            DetectBlinkFromEyes(eyes);
        }

        /// <summary>
        /// Detects pupil center within an eye region using thresholding and contours
        /// </summary>
        /// <param name="eyeRect">Rectangle defining the eye region</param>
        /// <returns>Pupil center position relative to face region</returns>
        private Vector2 DetectPupil(OpenCvSharp.Rect eyeRect)
        {
            OpenCvSharp.Rect absoluteEyeRect = new OpenCvSharp.Rect(
                faceRegion.X + eyeRect.X,
                faceRegion.Y + eyeRect.Y,
                eyeRect.Width,
                eyeRect.Height
            );

            Mat eyeROI = new Mat(grayFrame, absoluteEyeRect);
            Mat threshold = new Mat();

            Cv2.GaussianBlur(eyeROI, eyeROI, new Size(5, 5), 0);
            Cv2.Threshold(eyeROI, threshold, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
            Cv2.BitwiseNot(threshold, threshold);

            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(
                threshold, 
                out contours, 
                out hierarchy, 
                RetrievalModes.External, 
                ContourApproximationModes.ApproxSimple
            );

            if (contours.Length == 0)
            {
                return new Vector2(eyeRect.X + eyeRect.Width / 2, eyeRect.Y + eyeRect.Height / 2);
            }

            Point[] largestContour = contours.OrderByDescending(c => Cv2.ContourArea(c)).First();
            Moments moments = Cv2.Moments(largestContour);

            float pupilX = (float)(moments.M10 / moments.M00);
            float pupilY = (float)(moments.M01 / moments.M00);

            return new Vector2(
                eyeRect.X + pupilX,
                eyeRect.Y + pupilY
            );
        }

        /// <summary>
        /// Normalizes pupil position to 0-1 range based on eye region
        /// </summary>
        /// <param name="pupilPos">Raw pupil position</param>
        /// <param name="eyeRect">Eye region rectangle</param>
        private void UpdateNormalizedPosition(Vector2 pupilPos, OpenCvSharp.Rect eyeRect)
        {
            float normalizedX = Mathf.Clamp01(pupilPos.x / eyeRect.Width);
            float normalizedY = Mathf.Clamp01(pupilPos.y / eyeRect.Height);

            normalizedPupilPosition = new Vector2(normalizedX, normalizedY);
        }

        /// <summary>
        /// Applies smoothing to pupil position for stable tracking
        /// </summary>
        private void SmoothPosition()
        {
            smoothedPosition = Vector2.Lerp(
                normalizedPupilPosition,
                smoothedPosition,
                POSITION_SMOOTHING
            );
        }

        /// <summary>
        /// Detects blinks based on eye aspect ratio
        /// </summary>
        /// <param name="eyes">List of detected eye rectangles</param>
        private void DetectBlinkFromEyes(List<OpenCvSharp.Rect> eyes)
        {
            float eyeAspectRatio = CalculateAverageEyeAspectRatio(eyes);
            Debug.Log($"eyeAspectRatio: {eyeAspectRatio}");
            if (eyeAspectRatio < BLINK_THRESHOLD)
            {
                blinkFrameCounter++;
            }
            else
            {
                if (blinkFrameCounter >= BLINK_FRAME_COUNT && CanRegisterBlink())
                {
                    blinkDetectedThisFrame = true;
                    lastBlinkTime = Time.time;
                    Debug.Log($"blinkDetectedThisFrame - blinkFrameCounter:{blinkFrameCounter}");
                }
                blinkFrameCounter = 0;
            }
        }

        

        /// <summary>
        /// Calculates average eye aspect ratio from detected eyes
        /// </summary>
        /// <param name="eyes">List of detected eye rectangles</param>
        /// <returns>Average aspect ratio (height/width)</returns>
        private float CalculateAverageEyeAspectRatio(List<OpenCvSharp.Rect> eyes)
        {
            // float totalRatio = eyes.Sum(eye => (float)eye.Height / eye.Width);
            // return totalRatio / eyes.Count;
			if (eyes == null || eyes.Count == 0) return 0f;

			float totalRatio = 0f;
			for (int i = 0; i < eyes.Count; i++)
			{
				// Compute ratio from thresholded eye opening instead of Haar box shape
				totalRatio += GetEyeOpeningAspectRatio(eyes[i]);
			}

			return totalRatio / eyes.Count;
        }

		// Computes the eye opening aspect ratio for a single eye by thresholding the ROI
		private float GetEyeOpeningAspectRatio(OpenCvSharp.Rect eyeRect)
		{
			// Convert eyeRect to absolute frame coordinates
			OpenCvSharp.Rect absoluteEyeRect = new OpenCvSharp.Rect(
				faceRegion.X + eyeRect.X,
				faceRegion.Y + eyeRect.Y,
				eyeRect.Width,
				eyeRect.Height
			);

			if (absoluteEyeRect.Width <= 0 || absoluteEyeRect.Height <= 0) return 0f;

			using (Mat eyeROI = new Mat(grayFrame, absoluteEyeRect))
			using (Mat blurred = new Mat())
			using (Mat threshold = new Mat())
			{
				Cv2.GaussianBlur(eyeROI, blurred, new Size(5, 5), 0);
				Cv2.Threshold(blurred, threshold, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
				Cv2.BitwiseNot(threshold, threshold);

				return ComputeEyeOpeningAspectRatio(threshold);
			}
		}

		// Returns height/width of the largest dark region inside the thresholded eye ROI
		private float ComputeEyeOpeningAspectRatio(Mat threshold)
		{
			Point[][] contours;
			HierarchyIndex[] hierarchy;
			Cv2.FindContours(
				threshold,
				out contours,
				out hierarchy,
				RetrievalModes.External,
				ContourApproximationModes.ApproxSimple
			);

			if (contours == null || contours.Length == 0) return 0f;

			Point[] largest = contours.OrderByDescending(c => Cv2.ContourArea(c)).First();
			OpenCvSharp.Rect rect = Cv2.BoundingRect(largest);
			if (rect.Width == 0) return 0f;

			return (float)rect.Height / rect.Width;
		}

        /// <summary>
        /// Checks if enough time has passed since last blink
        /// </summary>
        /// <returns>True if blink can be registered</returns>
        private bool CanRegisterBlink()
        {
            return Time.time - lastBlinkTime >= BLINK_COOLDOWN;
        }

        /// <summary>
        /// Gets the current normalized eye position for crosshair control
        /// </summary>
        /// <returns>Smoothed eye position in 0-1 range</returns>
        public Vector2 GetNormalizedEyePosition()
        {
            return smoothedPosition;
        }

        /// <summary>
        /// Detects if a blink occurred this frame
        /// </summary>
        /// <returns>True if blink was detected</returns>
        public bool DetectBlink()
        {
            return blinkDetectedThisFrame;
        }

        /// <summary>
        /// Displays debug visualization window with detection results
        /// </summary>
        private void ShowDebugVisualization()
        {
            if (currentFrame.Empty()) return;

            Mat debugFrame = currentFrame.Clone();

            Cv2.Rectangle(debugFrame, faceRegion, Scalar.Green, 2);
            Cv2.PutText(
                debugFrame,
                $"Blink: {blinkDetectedThisFrame}",
                new Point(10, 30),
                HersheyFonts.HersheySimplex,
                0.7,
                Scalar.Red,
                2
            );

            Cv2.ImShow("Eye Tracker Debug", debugFrame);
            Cv2.WaitKey(1);
        }

        /// <summary>
        /// Cleanup resources on component destruction
        /// </summary>
        private void OnDestroy()
        {
            videoCapture?.Release();
            currentFrame?.Dispose();
            grayFrame?.Dispose();
            
            if (showDebugWindow)
            {
                Cv2.DestroyAllWindows();
            }
        }
    }
}