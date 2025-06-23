// Global variables
let sortColumn = "";
let sortOrder = "";
let searchTimeout;

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
            toastr.error("Internal server error.");
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
            toastr.error("Internal server error.");
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
            toastr.error("Internal server error.");
        },
    });
});

// Get friend request modal
function fetchAddExpenseModal(expenseId = 0) {
    $("#regularModalContent").empty();
    $.ajax({
        url: "/Expense/AddExpenseModal",
        type: "GET",
        data: {expenseId},
        success: function (response) {
            if (!response.statusCode) {
                $("#regularModalContent").html(response);
                $("#regularModal").modal("show");
            }
        },
        error: function () {
            $("#regularModal").modal("hide");
            toastr.error("Internal server error.");
        },
    });
}

// Export to excel
function exportDataAjax(filter, url, fileName) {
    $.ajax({
        url: url,
        type: "POST",
        data: { filter },
        xhrFields: {
            responseType: 'blob'
        },
        success: function (data, status, xhr) {
            if (data.success === false) {
                console.log(data)
                toastr.error(data.message);
            } else {
                let filename = `${fileName}_${new Date().getTime()}.xlsx`;

                let disposition = xhr.getResponseHeader('Content-Disposition');
                if (disposition && disposition.indexOf('attachment') !== -1) {
                    let matches = /filename="([^"]+)"/.exec(disposition);
                    if (matches !== null && matches[1]) filename = matches[1];
                }

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
            toastr.error(`No ${fileName.toLowerCase()} fround.`);
        }
    });
}
