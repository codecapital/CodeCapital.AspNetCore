"use strict";
function activateRipple() {
    document.addEventListener('mousedown', (e) => {
        const target = e.target;
        if (target.matches('[ripple], [ripple] *')) {
            e.preventDefault();
            const element = target.closest('[ripple]');
            const size = element.offsetWidth;
            const pos = element.getBoundingClientRect();
            const x = e.pageX - pos.left - (size / 2);
            const y = e.clientY - pos.top - (size / 2);
            const ripple = document.createElement('span');
            ripple.classList.add('ripple');
            ripple.setAttribute('style', 'top:' + y + 'px; left: ' + x + 'px; height: ' + size + 'px; width: ' + size + 'px;');
            const customColor = element.getAttribute('ripple-color');
            if (customColor)
                ripple.style.background = customColor;
            element.appendChild(ripple);
            setTimeout(() => {
                ripple.parentNode.removeChild(ripple);
            }, 500);
        }
    }, false);
}
var hasSomeParentTheAttribute = (element, attributeName) => {
    if (element && element.hasAttribute(attributeName))
        return true;
    return element.parentNode && hasSomeParentTheAttribute(element.parentNode, attributeName);
};
activateRipple();
window.CoreRipple = {
    activateRipple: function () {
        activateRipple();
    }
};
