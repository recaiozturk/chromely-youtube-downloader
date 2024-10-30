const connection = new signalR.HubConnectionBuilder().withUrl("/downloadHub").build();

async function Start() {

    try {
        await connection.start().then(() => {
        });
    } catch (err) {
        setTimeout(() => Start(), 3000)
    }
}

connection.onclose(async () => {
    await Start();
});

connection.on("ReceiveProgress", (id, progress) => {

    const progressBar = $(`#progress-bar-${id}`);
    const percent = Math.round(progress);

    progressBar.css("width", `${percent}%`);
    progressBar.attr("aria-valuenow", percent);
    progressBar.find('span').text(`${percent}%`);
});

$(document).ready(function () {

    Start();

    $("#grab-button").click(function () {

        const spinner = `<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Grabbing...`;
        const button = $(this);

        button.html(spinner);
        button.prop("disabled", true);

        const url = $("#url").val();

        $.ajax({
            url: "/Home/GrabVideoAndAudios",
            type: "POST",
            data: {
                url: url,
            },
            success: function (response) {
                if (response.isValid) {
                    $('#download-area').empty();
                    $.each(response.data, function (index, item) {

                        $('#download-area').append(`

                                                <div class="progress d-flex mt-3" style="height:40px">
                                                    <div id="progress-bar-${response.ids[index]}" class="progress-bar" role="progressbar" style="width: 0%" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100"><span class="position-absolute p-3" style=" color: black;">${item.title} - bitrate(${item.audioBitrate}) - ${response.audioFormats[index]}</span></div>
                                                    <input type="hidden" id="uri-${response.ids[index]}" value="${item.uri}">
                                                    <input type="hidden" id="title-${response.ids[index]}" value="${item.title}">
                                                    <button type="button" id="downloadButton-${response.ids[index]}" onclick="startDownload(this)" class="btn btn-success ms-auto"> Download</button>
                                                </div>

                                        `);
                    });
                } else {

                    const toastLiveExample = document.getElementById('liveToast')
                    const toastBootstrap = bootstrap.Toast.getOrCreateInstance(toastLiveExample)
                    document.getElementById("message-title").innerHTML = response.errorMessage;
                    document.getElementById("message-area").innerHTML = response.errorDescription;
                    toastBootstrap.show()
                }

                button.html("Grap Audios");
                button.prop("disabled", false);
            },
            error: function () {

                button.html("Grap Audios");
                button.prop("disabled", false);
            }
        });

    });
});

function startDownload(button) {

    const downloadId = $(button).attr('id').split('-').slice(1).join('-');
    const videoUri = $(`#uri-${downloadId}`).val();
    const title = $(`#title-${downloadId}`).val();


    const spinner = `<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Downloading...`;
    const $button = $(button);
    $button.html(spinner);
    $button.prop("disabled", true);


    $.ajax({
        url: "/Home/DownloadVideo",
        type: "POST",
        data: {
            audioUri: videoUri,
            downloadId: downloadId,
            title: title
        },
        success: function (data) {
            $button.html("Download");
            $button.prop("disabled", false);
        },
        error: function () {
            $button.html("Download");
            $button.prop("disabled", false);
        }
    });
}
