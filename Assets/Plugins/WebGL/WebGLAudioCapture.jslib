// WebGL Audio Capture Plugin

mergeInto(LibraryManager.library, {
    
    // Initialize audio capture
    WebGLAudio_Init: function(sampleRate, chunkSize) {
        window.webglAudio = {
            audioContext: null,
            mediaStream: null,
            scriptProcessor: null,
            sampleRate: sampleRate,
            chunkSize: chunkSize,
            audioBuffer: [],
            isCapturing: false,
            callback: null
        };
        console.log('WebGL Audio initialized');
    },

    // Start microphone capture
    WebGLAudio_StartMicrophone: async function(callbackPtr) {
        const audio = window.webglAudio;
        
        try {
            // Request microphone permission
            audio.mediaStream = await navigator.mediaDevices.getUserMedia({ 
                audio: {
                    sampleRate: audio.sampleRate,
                    channelCount: 1,
                    echoCancellation: true,
                    noiseSuppression: true
                } 
            });

            // Create audio context
            audio.audioContext = new (window.AudioContext || window.webkitAudioContext)({
                sampleRate: audio.sampleRate
            });

            const source = audio.audioContext.createMediaStreamSource(audio.mediaStream);
            
            // Create script processor (legacy but works)
            audio.scriptProcessor = audio.audioContext.createScriptProcessor(audio.chunkSize, 1, 1);
            
            audio.scriptProcessor.onaudioprocess = function(event) {
                if (!audio.isCapturing) return;
                
                const inputData = event.inputBuffer.getChannelData(0);
                
                // Convert float32 to int16 PCM
                const pcmData = new Int16Array(inputData.length);
                for (let i = 0; i < inputData.length; i++) {
                    const s = Math.max(-1, Math.min(1, inputData[i]));
                    pcmData[i] = s < 0 ? s * 0x8000 : s * 0x7FFF;
                }
                
                // Convert to byte array
                const bytes = new Uint8Array(pcmData.buffer);
                
                // Allocate Unity memory
                const bufferPtr = _malloc(bytes.length);
                HEAPU8.set(bytes, bufferPtr);
                
                // Call Unity callback
                if (callbackPtr) {
                    dynCall('vii', callbackPtr, [bufferPtr, bytes.length]);
                }
                
                // Free memory
                _free(bufferPtr);
            };
            
            source.connect(audio.scriptProcessor);
            audio.scriptProcessor.connect(audio.audioContext.destination);
            audio.isCapturing = true;
            
            console.log('Microphone started successfully');
            return 1; // Success
            
        } catch (error) {
            console.error('Failed to start microphone:', error);
            return 0; // Failure
        }
    },

    // Stop microphone capture
    WebGLAudio_StopMicrophone: function() {
        const audio = window.webglAudio;
        
        if (!audio) return;
        
        audio.isCapturing = false;
        
        if (audio.scriptProcessor) {
            audio.scriptProcessor.disconnect();
            audio.scriptProcessor = null;
        }
        
        if (audio.mediaStream) {
            audio.mediaStream.getTracks().forEach(track => track.stop());
            audio.mediaStream = null;
        }
        
        if (audio.audioContext) {
            audio.audioContext.close();
            audio.audioContext = null;
        }
        
        console.log('Microphone stopped');
    },

    // Check if microphone is available
    WebGLAudio_IsMicrophoneAvailable: function() {
        return navigator.mediaDevices && navigator.mediaDevices.getUserMedia ? 1 : 0;
    },

    // Get microphone permission status
    WebGLAudio_GetPermissionStatus: async function() {
        try {
            const result = await navigator.permissions.query({ name: 'microphone' });
            if (result.state === 'granted') return 2;
            if (result.state === 'denied') return 0;
            return 1; // prompt
        } catch (error) {
            return 1; // unknown, needs prompt
        }
    }
});