mergeInto(LibraryManager.library, {
    DownloadFile: function(filenamePtr, dataPtr, size) {
        // Convert pointers to JavaScript strings/data
        var filename = UTF8ToString(filenamePtr);
        var data = new Uint8Array(size);
        
        // Copy data from Unity memory
        for (var i = 0; i < size; i++) {
            data[i] = HEAPU8[dataPtr + i];
        }
        
        // Create blob and download link
        var blob = new Blob([data], { type: 'image/png' });
        var url = URL.createObjectURL(blob);
        
        // Create temporary download link and trigger click
        var link = document.createElement('a');
        link.href = url;
        link.download = filename;
        link.style.display = 'none';
        
        document.body.appendChild(link);
        link.click();
        
        // Cleanup
        setTimeout(function() {
            document.body.removeChild(link);
            URL.revokeObjectURL(url);
        }, 100);
    }
});