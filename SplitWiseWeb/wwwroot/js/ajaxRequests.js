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
                    // $("#partialViewContainer").html(response);
                    $("#right-section").html(response);
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

// Get friend request modal
function fetchAddFriendModal() {
    $("#regularModalContent").empty();
    $.ajax({
        url: "/Friend/AddFriendModal",
        type: "GET",
        success: function (response) {
            if (!response.statusCode) {
                $("#regularModalContent").html(response);
                $("#regularModal").modal("show");
            }
        },
        error: function () {
            $("#regularModal").modal("hide");
            toastr.error('@(NotificationMessages.CanNot.Replace("{0}", "add friend"))');
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
        error: function (xhr, status, error) {
            toastr.error("@NotificationMessages.InternalServerError");
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
        error: function (xhr, status, error) {
            toastr.error("@NotificationMessages.InternalServerError");
        },
    });
});

// Breadcrumb
$(document).ready(function () {
    $('body').on('click', '.breadcrumb a', function (e) {
        e.preventDefault();
        var url = $(this).attr('href');

        $.ajax({
            url: url,
            type: 'GET',
            success: function (data) {
                // $('#partialViewContainer').html(data);
                $('#right-section').html(data);
                // history.pushState(null, '', url);
            },
            error: function () {
                alert("Internal server error.");
            }
        });
    });
});