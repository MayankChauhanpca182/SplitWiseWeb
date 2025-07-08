// Get create new group modal
function fetchAddGroupModal(groupId = 0) {
    $("#regularModalContent").empty();
    $.ajax({
        url: "/Group/AddGroupModal",
        type: "GET",
        data: { groupId },
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

// Save group
$(document).on("submit", "#groupForm", function (e) {
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
                    $("#regularModal").modal("hide");
                    switch(window.location.pathname)
                    {
                        case "/groups":
                            getGroupList(1);
                            break;
                        case "/group-details":
                            location.reload();
                            break;
                    };
                }
                else if (response.success == false) {
                    toastr.error(response.message);
                }
                else {
                    $("#regularModalContent").html(response);
                }
            }
        },
        error: function (xhr, status, error) {
            toastr.error("Internal server error.");
        }
    });
});