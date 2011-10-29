/*global Node, FormData, $ */
var uploader;
var jqxhr;
var maxRetries = 1;
var operationType = {
    "METADATA_SEND": 0,
    "CANCELLED": 1,
    "RESUME_UPLOAD": 2,
    "METADATA_FAILED": 3,
    "FILE_NOT_SELECTED": 4
};

function ChunkedUploader(controlElements) {
    this.file = controlElements.fileControl.files[0];
    this.fileControl = controlElements.fileControl;
    this.statusLabel = controlElements.statusLabel;
    this.progressElement = controlElements.progressElement;
    this.uploadButton = controlElements.uploadButton;
    this.cancelButton = controlElements.cancelButton;
    this.totalBlocks = controlElements.totalBlocks;

    function isElementNode(node) {
        return !!(node.nodeType && node.nodeType === Node.ELEMENT_NODE);
    }

    function clearChildren(node) {
        if (isElementNode(node)) {
            while (node.firstChild) {
                node.removeChild(node.firstChild);
            }
        }
    }

    this.displayStatusMessage = function (message) {
        clearChildren(this.statusLabel);
        if (message) {
            this.statusLabel.appendChild(document.createTextNode(message));
        }
    };

    this.initializeUpload = function () {
        this.displayStatusMessage('');
        this.uploadButton.setAttribute('disabled', 'disabled');
        this.fileControl.setAttribute('disabled', 'disabled');
        this.cancelButton.removeAttribute('disabled');
    };

    this.resetControls = function () {
        this.progressElement.setAttribute('hidden', 'hidden');
        this.cancelButton.setAttribute('disabled', 'disabled');
        this.fileControl.removeAttribute('disabled');
        this.uploadButton.removeAttribute('disabled');
        this.fileControl.value = '';
    };

    this.displayLabel = function (operation) {
        switch (operation) {
            case operationType.METADATA_SEND:
                this.displayStatusMessage('Sending file metadata to server. Please wait.');
                break;
            case operationType.CANCELLED:
                this.displayStatusMessage('File upload has been cancelled.');
                break;
            case operationType.RESUME_UPLOAD:
                this.displayStatusMessage('Error encountered during upload. Resuming upload.');
                break;
            case operationType.METADATA_FAILED:
                this.displayStatusMessage('Failed to send file meta data. Retry after some time.');
                break;
            case operationType.FILE_NOT_SELECTED:
                this.displayStatusMessage('Please select a file.');
                break;
        }
    };

    this.uploadError = function (message) {
        this.displayStatusMessage('The file could not be uploaded because ' + message + '. Cancelling upload.');
        if (jqxhr !== null) {
            jqxhr.abort();
        }
    };

    this.renderProgress = function (blocksCompleted) {
        var percentCompleted = Math.floor((blocksCompleted - 1) * 100 / this.totalBlocks);
        this.progressElement.removeAttribute('hidden');
        this.progressElement.setAttribute('value', percentCompleted.toString());
        this.displayStatusMessage("Completed: " + percentCompleted + '%');
    };
}

function cancelUpload() {
    if (jqxhr !== null) {
        jqxhr.abort();
    }
}

var sendFile = function (blockLength) {
    var start = 0,
        end = Math.min(blockLength, uploader.file.size) - 1,
        incrimentalIdentifier = 1,
        retryCount = 0,
        sendNextChunk, fileChunk;
    uploader.displayStatusMessage('');
    sendNextChunk = function () {
        fileChunk = new FormData();
        uploader.renderProgress(incrimentalIdentifier);
        fileChunk.append('Slice', uploader.file.webkitSlice(start, end));
        jqxhr = $.ajax({
            async: true,
            url: ('/Home/UploadBlock/' + incrimentalIdentifier),
            data: fileChunk,
            cache: false,
            contentType: false,
            processData: false,
            type: 'POST',
            error: function (request, error) {
                if (error != 'abort' && retryCount < maxRetries) {
                    ++retryCount;
                    sendNextChunk();
                }

                if (error == 'abort') {
                    uploader.displayLabel(operationType.CANCELLED);
                    uploader.resetControls();
                    uploader = null;
                }
                else {
                    if (retryCount == maxRetries) {
                        uploader.uploadError(request.responseText);
                        uploader.resetControls();
                        uploader = null;
                    }
                    else {
                        uploader.displayLabel(operationType.RESUME_UPLOAD);
                    }
                }

                return;
            },
            success: function (notice) {
                if (notice.error || notice.isLastBlock) {
                    uploader.renderProgress(uploader.totalBlocks + 1);
                    uploader.displayStatusMessage(notice.message);
                    uploader.resetControls();
                    uploader = null;
                    return;
                }

                ++incrimentalIdentifier;
                start = (incrimentalIdentifier - 1) * blockLength;
                end = Math.min(incrimentalIdentifier * blockLength, uploader.file.size) - 1;
                retryCount = 0;
                sendNextChunk();
            }
        });
    };

    sendNextChunk();
};

function startUpload(fileElementId, blockLength, uploadProgressElement, statusLabel, uploadButton, cancelButton) {
    uploader = new ChunkedUploader({
        "fileControl": document.getElementById(fileElementId),
        "statusLabel": document.getElementById(statusLabel),
        "progressElement": document.getElementById(uploadProgressElement),
        "uploadButton": document.getElementById(uploadButton),
        "cancelButton": document.getElementById(cancelButton),
        "totalBlocks": 0
    });
    uploader.initializeUpload();
    if (uploader.file === null || uploader.file.size <= 0) {
        uploader.displayLabel(operationType.FILE_NOT_SELECTED);
        uploader.resetControls();
        return;
    }

    uploader.totalBlocks = Math.ceil(uploader.file.size / blockLength);
    uploader.progressElement.setAttribute('value', '0');
    uploader.displayLabel(operationType.METADATA_SEND);
    $.ajax({
        type: "POST",
        async: true,
        url: '/Home/PrepareMetaData',
        data: {
            'blocksCount': uploader.totalBlocks,
            'fileName': uploader.file.name,
            'fileSize': uploader.file.size
        },
        dataType: "json",
        error: function () {
            uploader.displayLabel(operationType.METADATA_FAILED);
            uploader.resetControls();
        },
        success: function () {
            sendFile(blockLength);
        }
    });
}