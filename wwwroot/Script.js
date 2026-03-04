window.closeMenuOnOutsideClick = (dotnetHelper) => {
      document.addEventListener("click",function(event){
            dotnetHelper.invokeMethodAsync("CloseMenu")
      })
}