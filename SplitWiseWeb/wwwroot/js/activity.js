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

$("#fromDate").blur(function () {
    // $("#toDate").attr("min", $(this).val());
    if (checkValidDate($(this))) {
        fetchActivities();
    }
});

$("#toDate").blur(function () {
    // $("#fromDate").attr("max", $(this).val());
    if (checkValidDate($(this))) {
        fetchActivities();
    }
});

function checkValidDate(element) {
    let min = $(element).attr("min");
    let max = $(element).attr("max");
    let date = $(element).val();

    console.log("min", min, "max", max);
    console.log("date", date);

    if (date) {
        if (date < min) {
            toastr.error(`Minimum allowed date is ${convertToDateFormate(min)}.`);
            $(element).val("");
            return false;
        }
        else if (date > max) {
            toastr.error(`Maximum allowed date is ${convertToDateFormate(max)}.`);
            $(element).val("");
            return false;
        }
    }
    return true;
}

function convertToDateFormate(dateStr) {
    const [yyyy, mm, dd] = dateStr.split("-");
    return `${dd}-${mm}-${yyyy}`;
}
