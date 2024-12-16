setTimeout(function () {
    var successMessage = document.getElementById("successMessage");
    var errorMessage = document.getElementById("errorMessage");

    if (successMessage) {
        successMessage.style.display = "none";
    }

    if (errorMessage) {
        errorMessage.style.display = "none";
    }
}, 1500);