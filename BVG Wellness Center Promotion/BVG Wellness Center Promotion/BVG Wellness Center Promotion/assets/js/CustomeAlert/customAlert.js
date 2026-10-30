function showCustomAlert(message, alertType, imageUrl) {
    // Create the alert box
    var alertBox = document.createElement('div');
    alertBox.className = `custom-alert ${alertType}`; // Add custom alert class and type-specific class

    // Set the content based on the alert type
    if (alertType === 'success') {
        // सक्सेस मेसेजसाठी इमेज पूर्णपणे काढून टाकली आहे, फक्त FontAwesome आयकॉन दिसेल
        alertBox.innerHTML = `<i class="fa fa-check-circle alert-icon" style="font-size: 24px; color: #28a745; margin-right: 8px;"></i> ${message}`;
    } else if (alertType === 'error') {
        // जर इमेज दिली असेल तर इमेज दाखवा, नाहीतर FontAwesome चा cross icon दाखवा
        if (imageUrl && imageUrl.trim() !== "") {
            alertBox.innerHTML = `<img src="${imageUrl}" alt="Error" class="alert-icon" /> ${message}`;
        } else {
            alertBox.innerHTML = `<i class="fa fa-times-circle alert-icon" style="font-size: 24px; color: #dc3545; margin-right: 8px;"></i> ${message}`;
        }
    } else {
        // Warning किंवा इतर अलर्टसाठी
        if (imageUrl && imageUrl.trim() !== "") {
            alertBox.innerHTML = `<img src="${imageUrl}" alt="Alert" class="alert-icon" /> ${message}`;
        } else {
            alertBox.innerHTML = `<i class="fa fa-exclamation-circle alert-icon" style="font-size: 24px; color: #ffc107; margin-right: 8px;"></i> ${message}`;
        }
    }

    // Append the alert to the body
    document.body.appendChild(alertBox);

    // Automatically hide the alert after 5 seconds
    setTimeout(() => {
        alertBox.classList.add('fade-out');
        setTimeout(() => {
            alertBox.remove(); // Remove from DOM after fade-out
        }, 500); // Match the CSS fade-out duration
    }, 5000);
}
