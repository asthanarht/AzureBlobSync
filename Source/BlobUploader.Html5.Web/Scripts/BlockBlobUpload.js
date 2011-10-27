var file = null;
var fileControl = null;
var statusLabel = null;
var progressElement = null;
var uploadButton = null;
var cancelButton = null;
var jqxhr = null;
this.totalBlocks = 0;

function cancelUpload() {
    if (this.jqxhr != null) {
        this.jqxhr.abort();
    }
}

var sendFile = function (blockLength) {
    var start = 0, end = Math.min(blockLength, file.size) - 1, incrimentalIdentifier = 1, sendNextChunk, fileChunk = null;
    this.statusLabel.innerHTML = '';
    sendNextChunk = function () {
        fileChunk = new FormData();
        fileChunk.append(incrimentalIdentifier, file.webkitSlice(start, end));
        this.progressElement.setAttribute('value', Math.floor((incrimentalIdentifier - 1) * 100 / this.totalBlocks).toString());
        this.jqxhr = $.ajax({
            async: true,
            url: '/Home/UploadBlock',
            context: this,
            data: fileChunk,
            cache: false,
            contentType: false,
            processData: false,
            type: 'POST',
            error: function (request, error) {
                this.statusLabel.innerHTML = error == 'abort' ? 'File upload has been cancelled.' : 'The file could not be uploaded because ' + error;
                this.fileControl.value = '';
                resetControls();
                return;
            },
            success: function (notice) {
                if (notice.error || notice.isLastBlock) {
                    this.statusLabel.innerHTML = notice.message;
                    this.fileControl.value = '';
                    resetControls();
                    return;
                }

                start = end + 1;
                end = Math.min(start + blockLength, file.size) - 1;
                ++incrimentalIdentifier;
                sendNextChunk();
            }
        });
    };

    sendNextChunk();
};

function startUpload(fileElementId, blockLength, uploadProgressElement, statusLabel, uploadButton, cancelButton) {
    this.file = document.getElementById(fileElementId).files[0];
    this.fileControl = document.getElementById(fileElementId);
    this.statusLabel = document.getElementById(statusLabel);
    this.progressElement = document.getElementById(uploadProgressElement);
    this.uploadButton = document.getElementById(uploadButton); ;
    this.cancelButton = document.getElementById(cancelButton); ;
    this.blockLength = blockLength;

    this.statusLabel.innerHTML = '';
    this.uploadButton.setAttribute('disabled', 'disabled');
    this.fileControl.setAttribute('disabled', 'disabled');
    this.cancelButton.removeAttribute('disabled');
    if (this.file == null || this.file.size <= 0) {
        this.statusLabel.innerHTML = 'Please select a file.';
        resetControls();
        return;
    }

    this.totalBlocks = Math.ceil(file.size / blockLength);
    this.progressElement.removeAttribute('hidden');
    this.progressElement.setAttribute('value', '0');
    this.statusLabel.innerHTML = 'Sending file metadata to server.';
    $.ajax({
        type: "POST",
        context: this,
        async: true,
        url: '/Home/PrepareMetaData',
        data: { 'blocksCount': this.totalBlocks, 'fileName': this.file.name, 'fileSize': this.file.size },
        dataType: "json",
        error: function () {
            this.statusLabel.innerHTML = 'Failed to send file meta data. Retry after some time.';
            this.fileControl.value = '';
            resetControls();
        },
        success: sendFile(blockLength)
    });
}

function resetControls() {
    this.progressElement.setAttribute('hidden', 'hidden');
    this.cancelButton.setAttribute('disabled', 'disabled');
    this.fileControl.removeAttribute('disabled');
    this.uploadButton.removeAttribute('disabled');
}