

$(document).ready(function () {
    // Configuración del estilo de la barra de navegación
    var navbarStyle = window.config.config.phoenixNavbarStyle;
    if (navbarStyle && navbarStyle !== 'transparent') {
        $('body').addClass(`navbar-${navbarStyle}`);
    }

    // Configuración de la dirección del texto (RTL o LTR)
    var phoenixIsRTL = window.config.config.phoenixIsRTL;
    if (phoenixIsRTL) {
        $('#style-default, #user-style-default').attr('disabled', true);
        $('html').attr('dir', 'rtl');
    } else {
        $('#style-rtl, #user-style-rtl').attr('disabled', true);
    }

    // Configuración del estilo de la barra de navegación superior
    var navbarTopStyle = window.config.config.phoenixNavbarTopStyle;
    if (navbarTopStyle === 'darker') {
        $('.navbar-top').addClass('navbar-darker');
    }

    // Configuración del estilo de la barra de navegación vertical
    var navbarVerticalStyle = window.config.config.phoenixNavbarVerticalStyle;
    var navbarVertical = $('.navbar-vertical');
    if (navbarVertical.length && navbarVerticalStyle === 'darker') {
        navbarVertical.addClass('navbar-darker');
    }

    function updateLogo() {
        let bgColor = $("body").css("background-color");
        let isDarkMode = getLuminance(bgColor) < 0.5; // Verifica si el fondo es oscuro

        if (isDarkMode) {
            $(".logo-dark").removeClass("d-none");
            $(".logo-light").addClass("d-none");
        } else {
            $(".logo-dark").addClass("d-none");
            $(".logo-light").removeClass("d-none");
        }
    }

    function getLuminance(rgb) {
        let rgbValues = rgb.match(/\d+/g).map(Number);
        let [r, g, b] = rgbValues.map(v => v / 255);
        return 0.2126 * r + 0.7152 * g + 0.0722 * b;
    }

    // Ejecutar al cargar la página
    updateLogo();

    // Verificar cambios de fondo cada 500ms (por si el tema cambia dinámicamente)
    setInterval(updateLogo, 50);
});