// Global variables
let sortColumn = "";
let sortOrder = "";
let searchTimeout;

// Store in session storage
function storeInSession(endPoint, navId) {
    sessionStorage.setItem("endPoint", endPoint);
    sessionStorage.setItem("navId", navId);
}

// Breadcrumb
$(document).ready(function () {
    $('body').on('click', '.breadcrumb a', function (e) {
        e.preventDefault();
        let endPoint = $(this).attr('href').substring(1);
        let navId;

        switch (endPoint) {
            case "home":
                endPoint = 'dashboard';
                navId = "dashboardPageNav";
                break;
            case "dashboard":
                navId = "dashboardPageNav";
                break;
            case "changePassword":
                navId = "changePasswordNav";
                break;
            case "profile":
                navId = "profileNav";
                break;
            case "friends":
                navId = "friendsPageNav";
                break;
            case "friendRequests":
                navId = "friendRequestsPageNav";
                break;
            case "groups":
                navId = "groupsPageNav";
                break;
        }

        getPage(endPoint, navId);

        // $.ajax({
        //     url: endPoint,
        //     type: 'GET',
        //     success: function (data) {
        //         $('#right-section').html(data);
        //         $(".navItems").removeClass("active");
        //         $(`.navItems#${navId}`).addClass("active");

        //         // Store into session storage
        //         storeInSession(endPoint, navId);
        //         // history.pushState(null, '', endPoint);
        //     },
        //     error: function () {
        //         alert("Internal server error.");
        //     }
        // });
    });
});

// Fetch pages through ajax
function getPage(endPoint, navId) {
    console.log(endPoint, " ", navId)

    $.ajax({
        url: `/${endPoint}`,
        type: "GET",
        success: function (response) {
            if (!response.statusCode) {
                if (response.success == false) {
                    toastr.error(response.message);
                }
                else {
                    $(".navItems").removeClass("active");
                    $(`.navItems#${navId}`).addClass("active");
                    $("#right-section").html(response);

                    // Store into session storage
                    storeInSession(endPoint, navId);
                    // history.pushState(null, '', endPoint);
                }
            }
        },
        error: function () {
            toastr.error("Internal server error.");
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
            toastr.error("Internal server error.");
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
            toastr.error("Internal server error.");
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
