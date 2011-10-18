
function startUpload(fileElementId, blockLength, uploadProgressElement) {
    var totalNumberOfBlocks = 0;
    var file = document.getElementById(fileElementId).files[0];
    //var progressBar = document.getElementById(uploadProgressElement);

    if (file.size <= 0) {
        alert('Please select a file!');
        return;
    }

    if ((file.size % blockLength) == 0) {
        totalNumberOfBlocks = parseInt(file.size / blockLength);
    }
    else {
        totalNumberOfBlocks = parseInt(file.size / blockLength) + 1;
    }

    $.ajax({
        type: "POST",
        async: false,
        url: "/Home/PrepareMetaData",
        data: { "blocksCount": totalNumberOfBlocks, "fileName": file.name, "fileSize": file.size },
        dataType: "json",
        success: function () {
            var start = 0;
            var end = Math.min(blockLength, file.size - 1);
            var finalBlock = false;
            var incrimentalIdentifier = 1;
            while (end <= file.size - 1) {
                var xhr = new XMLHttpRequest();
                xhr.open('POST', "/Home/UploadBlock", false);
                xhr.setRequestHeader('X-File-Name', incrimentalIdentifier);
                xhr.setRequestHeader('Content-Type', file.type);
                //                xhr.upload.addEventListener('progress', function (e) {
                //                    var percent = parseInt(e.loaded / e.total * 100);
                //                    progressBar.value = percent;
                //                }, false);
                try {
                    xhr.send(file.webkitSlice(start, end));
                }
                catch (e) {
                    alert(e.Message);
                    break;
                }

                if (!finalBlock) {
                    start = end + 1;
                    end = Math.min(start + blockLength, file.size) - 1;
                    incrimentalIdentifier++;
                    if (end == file.size - 1) {
                        finalBlock = true;
                    }
                }
                else {
                    break;
                }
            }
        },
        error: function () {
            alert('Failed to send file meta data');
        }
    });

    return false;
}