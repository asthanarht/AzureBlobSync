
function startUpload(fileElementId, blockLength, uploadProgressElement, statusLabel) {
    var totalNumberOfBlocks = 0;
    var file = document.getElementById(fileElementId).files[0];
    var statusLabel = document.getElementById(statusLabel);
    var progressElement = document.getElementById(uploadProgressElement);
    statusLabel.innerHTML = '';
    if (file.size <= 0) {
        statusLabel.innerHTML = 'Please select a file!';
        return;
    }

    totalNumberOfBlocks = Math.ceil(file.size / blockLength);
    progressElement.removeAttribute('hidden');
    $.ajax({
        type: "POST",
        async: true,
        url: "/Home/PrepareMetaData",
        data: { "blocksCount": totalNumberOfBlocks, "fileName": file.name, "fileSize": file.size },
        dataType: "json",
        error: function () {
            statusLabel.innerHTML = 'Failed to send file meta data';
            progressElement.setAttribute('hidden', 'hidden');
        },
        success: function () {
            var start = 0;
            var end = Math.min(blockLength, file.size) - 1;
            var incrimentalIdentifier = 1;
            while (start <= file.size - 1) {
                var data = new FormData();
                data.append(incrimentalIdentifier, file.webkitSlice(start, end));
                $.ajax({
                    async: true,
                    url: '/Home/UploadBlock',
                    data: data,
                    cache: false,
                    contentType: false,
                    processData: false,
                    type: 'POST',
                    error: function (notice) {
                        statusLabel.innerHTML = notice;
                        progressElement.setAttribute('hidden', 'hidden');
                        return false;
                    },
                    success: function (notice) {
                        if (notice.error || notice.isLastBlock) {
                            statusLabel.innerHTML = notice.message;
                            progressElement.setAttribute('hidden', 'hidden');
                            return false;
                        }
                    }
                });

                start = end + 1;
                end = Math.min(start + blockLength, file.size) - 1;
                incrimentalIdentifier++;
            }
        }
    });
}