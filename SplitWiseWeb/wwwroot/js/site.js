// Reinitialize the jquery validation
function reinitializeValidation() {
    $("form").each(function () {
        $.validator.unobtrusive.parse($(this));
    });
}

// Call this function after any AJAX request that adds forms dynamically
$(document).ajaxComplete(function () {
    reinitializeValidation();
});

// Apply validation on input change globally
$(document).on("keyup change", "form input:not([type=checkbox]):not([type=radio]), form select, form textarea", function () {
    $(this).valid();
});

// Prevent submission if validation fails
$(document).on("submit", "form", function (e) {
    if (!$(this).valid()) {
        e.preventDefault();
    }
});

// Loading Spinner
$("#loader").show();

$(document).ready(function () {
  $("#loader").hide();

  $(document).ajaxStop(function () {
    $("#loader").hide();
  });

  $(document).on("submit", "form", function (e) {
    $("#loader").show();
  });

  $(window).on("load", function () {
    $("#loader").hide();
  });
});