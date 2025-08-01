// Search input
$("#activitySearch").on("input", function () {
    if (searchTimeout === null) {
        searchTimeout = setTimeout(function () {
            fetchActivities();
        }, 500);
    }
});

$(document).ready(function () {
    fetchActivities();
});

function searchActivities() {
    if (validateDates()) {
        fetchActivities();
    }
}

function exportActivitiesValidate(){
    if (validateDates()) {
        exportActivities();
    }
}

function validateDates() {
    let fromDate = $("#fromDate").val();
    let toDate = $("#toDate").val();

    if (!fromDate && !toDate) {
        toastr.error("Select From date And To date.");
        return false;
    }
    else if (!fromDate) {
        toastr.error("Select From date.");
        return false;
    }
    else if (!toDate) {
        toastr.error("Select To date.");
        return false;
    }

    if (fromDate && toDate) {
        if (fromDate > toDate) {
            toastr.error("To Date should be greater then From Date.");
            $("#toDate").val(fromDate).trigger("blur");
            return false;
        }
    }
    
    return true;
}
