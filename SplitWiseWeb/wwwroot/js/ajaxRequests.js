// Global variables
let sortColumn = "";
let sortOrder = "";
let searchTimeout;

// Fetch pages through ajax
function getPage(endpoint, navId) {
    $.ajax({
        url: `/${endpoint}`,
        type: "GET",
        success: function (response) {
            if (!response.statusCode) {
                if (response.success == false) {
                    toastr.error(response.message);
                }
                else {
                    $(".navItems").removeClass("active");
                    $(`.navItems#${navId}`).addClass("active");
                    $("#partialViewContainer").html(response);
                }
            }
        },
        error: function () {
            toastr.error('@NotificationMessages.InternalServerError');
        }
    });
}

// Update profile
$(document).on("submit", "#profileForm", function (e) {
    e.preventDefault();

    let formData = new FormData(this);

    $.ajax({
        url: $(this).attr("action"),
        type: $(this).attr("method"),
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            if (!response.statusCode) {
                if (response.success) {
                    toastr.success(response.message);
                }
                else if (response.success == false) {
                    toastr.error(response.message);
                }
                else {
                    $("#partialVewContainer").html(response);
                }
            }
        },
        error: function (xhr, status, error) {
            toastr.error('@NotificationMessages.InternalServerError');
        }
    });
});

// Change password
$(document).on("submit", "#changePasswordForm", function (e) {
    e.preventDefault();

    let formData = new FormData(this);

    $.ajax({
        url: $(this).attr("action"),
        type: $(this).attr("method"),
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            if (!response.statusCode) {
                if (response.success) {
                    console.log("success")
                    toastr.success(response.message);
                    window.location = "/logout";
                }
                else if (response.success == false) {
                    toastr.error(response.message);
                }
                else {
                    $("#partialVewContainer").html(response);
                }
            }
        },
        error: function (xhr, status, error) {
            toastr.error('@NotificationMessages.InternalServerError');
        }
    });
});