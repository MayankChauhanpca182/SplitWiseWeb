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

$("#fromDate").change(function () {
    $("#toDate").attr("min", $(this).val());
    fetchActivities();
});

$("#toDate").change(function () {
    $("#fromDate").attr("max", $(this).val());
    fetchActivities();
});
