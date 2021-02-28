function activateRipple() {

  document.addEventListener('mousedown', (e: MouseEvent) => {

    const target = e.target as HTMLElement;

    if (target.matches('[ripple], [ripple] *')) {

      e.preventDefault();

      const element = target.closest('[ripple]') as HTMLElement;

      const size = element.offsetWidth;
      const pos = element.getBoundingClientRect();
      const x = e.pageX - pos.left - (size / 2);
      const y = e.clientY - pos.top - (size / 2);

      //const y = e.pageY - pos.top - (size / 2);
      //const y = pos.top + (e.clientY - pos.top);
      //const y = element.offsetHeight;//- e.clientY + pos.top;
      //const y = e.clientY - pos.top;
      //const y = e.clientY - pos.top - (element.offsetHeight / 2);

      //console.log(`${e.clientY}: ${pos.top}`);
      //console.log(`x: ${e.pageX}, y: ${e.pageY}, x1: ${pos.left}, y1: ${pos.top}, clientY: ${e.clientY}, diffY: ${e.clientY - pos.top - (size / 2)}`);

      const ripple = document.createElement('span') as HTMLElement;
      ripple.classList.add('ripple');
      ripple.setAttribute('style', 'top:' + y + 'px; left: ' + x + 'px; height: ' + size + 'px; width: ' + size + 'px;');

      const customColor = element.getAttribute('ripple-color');
      if (customColor) ripple.style.background = customColor;

      //ripple.className = 'ripple';
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
}

activateRipple();

(window as any).CoreRipple = {
  activateRipple: function() {
    activateRipple();
  }
};
