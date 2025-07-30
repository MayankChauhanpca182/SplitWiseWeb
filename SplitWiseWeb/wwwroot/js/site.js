// Global variables
const SERVER_ERR = "Internal server error."

// Prevent bfcache for chrome
window.addEventListener('pageshow', function (event) {
  if (event.persisted) {
    $("#loader").hide();
  }
});

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

  $(document).on("submit", ".loaderForm", function (e) {
    $("#loader").show();
  });

  $(document).on("click", "a:not(.no-loader)", function () {
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
    const existingTooltip = bootstrap.Tooltip.getInstance(this);
    if (existingTooltip) {
      existingTooltip.dispose();
    }

    new bootstrap.Tooltip(this, {
      html: true,
      placement: "top",
      trigger: "hover",
      delay: { show: 100, hide: 50 },
      container: "body",
      popperConfig(defaultBsPopperConfig) {
        return {
          ...defaultBsPopperConfig,
          modifiers: [
            {
              name: "alignLeftExactly",
              enabled: true,
              phase: "write",
              fn({ state }) {
                if (state.placement === "top") {
                  const tdLeft = state.elements.reference.getBoundingClientRect().left;
                  const scrollLeft = window.pageXOffset || document.documentElement.scrollLeft;
                  state.elements.popper.style.left = `${tdLeft + scrollLeft}px`;
                }
              },
            },
            {
              name: "flip",
              options: {
                fallbackPlacements: ["right", "left", "bottom"],
              },
            },
            {
              name: "preventOverflow",
              options: {
                boundary: "viewport",
              },
            },
            {
              name: "computeStyles",
              options: {
                gpuAcceleration: false,
              },
            },
          ],
        };
      },
    });
  });
}

$(document).ready(function () {
  $("td").each(function () {
    let text = $(this).text();
    $(this).attr("title", text.trim());
  });
  initializeTooltips();
});

$(document).ajaxComplete(function () {
  $(".tooltip").remove();
  $("td").each(function () {
    let text = $(this).text();
    $(this).attr("title", text.trim());
  });
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
  oldvalue = $(this).val();
});

// Format to INR
function formatToINR(amount) {
  let formatedAmount = new Intl.NumberFormat("en-IN", {
    style: "currency",
    currency: "INR",
    maximumFractionDigits: 2,
  }).format(amount);
  return formatedAmount.replace("₹", "");
}

// Search in dropdown
$(document).on("input", ".dropdownSearch", function () {
  let searchBox = $(this);
  let searchStr = searchBox.val().toLowerCase().replace(/\s/g, "");
  searchBox.parent().parent().find("li").hide();

  if (searchStr === "") {
    searchBox.parent().parent().find("li").show();
  }
  else {
    $(".dropdownList span.name").each(function () {
      let name = $(this).text().toLowerCase().replace(/\s/g, "");
      if (name.includes(searchStr)) {
        $(this).closest(".dropdownLi").show();
      }
    });
  }
  checkMasterCheckbox();
});

// Empty searchbox on dropdown open
function emptySearchBox() {
  $(".dropdownSearch").val("").trigger("input");
}

// Drop down master checkbox
$(document).on("change", "#userCkbMaster", function () {
  let checkedMaster = $(this).prop("checked");

  $(".userCkb").filter(function () {
    return $(this).closest("li").is(":visible");
  }).each(function () {
    if ($(this).prop("checked") !== checkedMaster) {
      $(this).prop("checked", checkedMaster).trigger("change");
    }
  });
});

$(document).on("change", ".userCkb", function () {
  checkMasterCheckbox();
});

function checkMasterCheckbox() {
  let totalSubCheckBox = $(".userCkb").filter(function () {
    return $(this).closest("li").is(":visible");
  }).length;

  let checkedSubCheckBox = $(".userCkb:checked").filter(function () {
    return $(this).closest("li").is(":visible");
  }).length;

  if (checkedSubCheckBox === totalSubCheckBox) {
    $("#userCkbMaster").prop("indeterminate", false).prop("checked", true);
  }
  else if (checkedSubCheckBox === 0) {
    $("#userCkbMaster").prop("indeterminate", false).prop("checked", false);
  }
  else {
    $("#userCkbMaster").prop("indeterminate", true);
  }
}

// Back button click
$(document).on("click", ".back-btn", function () {
  if (document.referrer) {
    window.history.back();
  }
  else {
    window.location.href = "/";
  }
});