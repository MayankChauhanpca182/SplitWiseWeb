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
$(document).on(
  "keyup change",
  "form input:not([type=checkbox]):not([type=radio]), form select, form textarea",
  function () {
    $(this).valid();
  }
);

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

  $(document).on("submit", "form", function (e) {
    $("#loader").show();
  });

  $(document).on("click", "a", function () {
    $("#loader").show();
  });

  $(document).ajaxStart(function () {
    $("#loader").show();
  });

  $(document).ajaxStop(function () {
    $("#loader").hide();
  });
});

// Tool tips
function initializeTooltips() {
  $("[title]").each(function () {
    // Destroy any existing tooltip to prevent duplicates
    const existingTooltip = bootstrap.Tooltip.getInstance(this);
    if (existingTooltip) {
      existingTooltip.dispose();
    }

    new bootstrap.Tooltip(this, {
      html: true,
      placement: "top",
      trigger: "hover",
      delay: { show: 100, hide: 50 },
      popperConfig: function (defaultBsPopperConfig) {
        return {
          ...defaultBsPopperConfig,
          modifiers: [
            {
              name: "offset",
              options: {
                offset: [1, 1],
              },
            },
            {
              name: "flip",
              options: {
                fallbackPlacements: ["right", "left", "bottom"],
              },
            },
          ],
        };
      },
    });
  });
}

$(document).ready(function () {
  initializeTooltips();
});

$(document).ajaxComplete(function () {
  $(".tooltip").remove();
  initializeTooltips();
});

//  Toggle Sidebar
$(document).on("click", "#hamBurger", function () {
  if ($("#navigation").css("display") == "none") {
    $("#navigation").css("display", "block");
    $("#right-section").css("width", "calc(100% - 200px)");
  } else {
    $("#navigation").css("display", "none");
    $("#right-section").css("width", "100%");
  }
});

// On focus select value
$(document).on("focus", "input", function () {
  $(this).select();
});

// Format to INR
function formatToINR(amount) {
  return new Intl.NumberFormat("en-IN", {
    style: "currency",
    currency: "INR",
    maximumFractionDigits: 2,
  }).format(amount);
}

