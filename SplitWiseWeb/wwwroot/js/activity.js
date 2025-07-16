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

$("#fromDate, #toDate").blur(function () {
    if (checkValidDate($(this))) {
        fetchActivities();
    }
});

function checkValidDate(element) {
    let min = $(element).attr("min");
    let max = $(element).attr("max");
    let date = $(element).val();

    if (date < min) {
        toastr.error(`Minimum allowed date is ${convertToDateFormate(min)}.`);
        $(element).val(min);
        return false;
    }
    else if (date > max) {
        toastr.error(`Maximum allowed date is ${convertToDateFormate(max)}.`);
        $(element).val(max);
        return false;
    }
    else{
        return true;
    }
}

function convertToDateFormate(dateStr) {
    const [yyyy, mm, dd] = dateStr.split("-");
    return `${dd}-${mm}-${yyyy}`;
}
