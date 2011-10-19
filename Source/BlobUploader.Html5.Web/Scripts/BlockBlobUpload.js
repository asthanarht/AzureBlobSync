
function startUpload(fileElementId, blockLength, uploadProgressElement) {
    var totalNumberOfBlocks = 0;
    var file = document.getElementById(fileElementId).files[0];
    if (file.size <= 0) {
        alert('Please select a file!');
        return;
    }

    totalNumberOfBlocks = Math.ceil(file.size / blockLength);
    $.ajax({
        type: "POST",
        async: false,
        url: "/Home/PrepareMetaData",
        data: { "blocksCount": totalNumberOfBlocks, "fileName": file.name, "fileSize": file.size },
        dataType: "json",
        error: function () {
            alert('Failed to send file meta data');
        },
        success: function () {
            var start = 0;
            var end = Math.min(blockLength, file.size) - 1;
            var incrimentalIdentifier = 1;
            while (start <= file.size - 1) {
                var xhr = new XMLHttpRequest();
                xhr.open('POST', "/Home/UploadBlock", false);
                xhr.setRequestHeader("Cache-Control", "no-cache");
                xhr.setRequestHeader('X-File-Name', incrimentalIdentifier);
                xhr.setRequestHeader('Content-Type', "multipart/form-data");
                xhr.send(file.webkitSlice(start, end));
                start = end + 1;
                end = Math.min(start + blockLength, file.size) - 1;
                incrimentalIdentifier++;
            }
        }
    });
}