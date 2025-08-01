// Global variables
let sortColumn = "";
let sortOrder = "";
let searchTimeout;

// Get friend request modal
function fetchAddFriendModal() {
    $("#regularModalContent").empty();
    $.ajax({
        url: baseUrl + "Friend/AddFriendModal",
        type: "GET",
        success: function (response) {
            if (!response.statusCode) {
                $("#regularModalContent").html(response);
                $("#regularModal").modal("show");
            }
        },
        error: function () {
            $("#regularModal").modal("hide");
            toastr.error(SERVER_ERR);
        },
    });
}

// Submit friend request form
$(document).on("submit", "#addFriendRequestForm", function (e) {
    e.preventDefault();

    $.ajax({
        url: $(this).attr("action"),
        type: $(this).attr("method"),
        data: $(this).serialize(),
        success: function (response) {
            if (!response.statusCode) {
                if (response.success) {
                    $("#regularModal").modal("hide");
                    toastr.success(response.message);
                } else if (response.success == false) {
                    toastr.error(response.message);
                } else {
                    $("#regularModalContent").html(response);
                }
            }
        },
        error: function () {
            toastr.error(SERVER_ERR);
        },
    });
});

// Submit referral form
$(document).on("submit", "#sendReferralForm", function (e) {
    e.preventDefault();

    $.ajax({
        url: $(this).attr("action"),
        type: $(this).attr("method"),
        data: $(this).serialize(),
        success: function (response) {
            if (!response.statusCode) {
                if (response.success) {
                    $("#regularModal").modal("hide");
                    toastr.success(response.message);
                } else if (response.success == false) {
                    toastr.error(response.message);
                } else {
                    $("#regularModalContent").html(response);
                }
            }
        },
        error: function () {
            toastr.error(SERVER_ERR);
        },
    });
});

// Export to excel
function exportExcelAjax(filter, url, fileName, groupId = null, friendUserId = null) {
    $.ajax({
        url: baseUrl + url,
        type: "POST",
        data: { filter, groupId, friendUserId },
        xhrFields: {
            responseType: 'blob'
        },
        success: function (data, status, xhr) {
            if (data.success === false) {
                toastr.error(data.message);
            } else {
                let filename = `${fileName}_${new Date().getTime()}.xlsx`;

                let blob = new Blob([data], { type: xhr.getResponseHeader('Content-Type') });
                let link = document.createElement('a');
                link.href = window.URL.createObjectURL(blob);
                link.download = filename;
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);

                toastr.success(`${fileName} has been exported successfully.`);
            }
        },
        error: function () {
            toastr.error(`No records found.`);
        }
    });
}

