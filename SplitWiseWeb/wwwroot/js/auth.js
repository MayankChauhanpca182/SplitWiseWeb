// Toggle Password Visibility
function togglePasswordVisibility(element) {
    let eyeButton = $(element);
    let password = $(element).closest(".input-group").children("input");
    if (password.prop("type") === "password") {
        password.prop("type", "text")
        eyeButton.prop("src", "/images/icons/eye.png");
    } else {
        password.prop("type", "password")
        eyeButton.prop("src", "/images/icons/eye-closed.png");
    }
}