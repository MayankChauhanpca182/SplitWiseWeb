// Toggle Password Visibility
function togglePasswordVisibility(element) {
    let eyeButton = $(element);
    let password = $(element).closest(".input-group").children("input");
    if (password.prop("type") === "password") {
        password.prop("type", "text")
        eyeButton.removeClass("bi-eye-slash-fill").addClass("bi-eye-fill");
        
    } else {
        password.prop("type", "password")
        eyeButton.removeClass("bi-eye-fill").addClass("bi-eye-slash-fill");
    }
}