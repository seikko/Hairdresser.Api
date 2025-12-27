// Confirmation dialog for important actions
document.addEventListener('DOMContentLoaded', function() {
    // Add confirmation to cancel buttons
    const cancelForms = document.querySelectorAll('form[action*="UpdateStatus"] input[value="cancelled"]');
    cancelForms.forEach(input => {
        const form = input.closest('form');
        form.addEventListener('submit', function(e) {
            if (!confirm('Bu randevuyu iptal etmek istediğinizden emin misiniz?')) {
                e.preventDefault();
            }
        });
    });

    // Auto-close alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
    alerts.forEach(alert => {
        setTimeout(() => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });

    // Add loading spinner to buttons on submit
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function() {
            const submitBtn = form.querySelector('button[type="submit"]');
            if (submitBtn) {
                submitBtn.disabled = true;
                const originalText = submitBtn.innerHTML;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>İşleniyor...';
                
                // Re-enable after 3 seconds as fallback
                setTimeout(() => {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalText;
                }, 3000);
            }
        });
    });

    // Phone number click to copy
    const phoneLinks = document.querySelectorAll('a[href^="tel:"]');
    phoneLinks.forEach(link => {
        link.addEventListener('contextmenu', function(e) {
            e.preventDefault();
            const phone = this.textContent.trim();
            navigator.clipboard.writeText(phone).then(() => {
                alert('Telefon numarası kopyalandı: ' + phone);
            });
        });
    });
});

// Refresh page every 5 minutes to keep data fresh
setTimeout(() => {
    location.reload();
}, 300000);

