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
    let fromDate = $("#fromDate").val();
    let toDate = $("#toDate").val();

    if (validateDates()) {
        fetchActivities();
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

    if (checkValidDate($("#fromDate")) && checkValidDate($("#toDate"))) {
        return true;
    }
    else {
        return false;
    }
}

function checkValidDate(element) {
    let min = $(element).attr("min");
    let max = $(element).attr("max");
    let date = $(element).val();

    if (date < min) {
        toastr.error(`Minimum allowed date is ${convertToDateFormate(min)}.`);
        return false;
    }
    else if (date > max) {
        toastr.error(`Maximum allowed date is ${convertToDateFormate(max)}.`);
        return false;
    }
    else {
        return true;
    }
}

function convertToDateFormate(dateStr) {
    const [yyyy, mm, dd] = dateStr.split("-");
    return `${dd}-${mm}-${yyyy}`;
}
