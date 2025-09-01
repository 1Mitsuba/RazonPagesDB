// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", function () {
    // Sidebar toggle
    const sidebarToggle = document.getElementById("sidebarToggle");
    
    if (sidebarToggle) {
        sidebarToggle.addEventListener("click", function () {
            document.body.classList.toggle("sidebar-collapsed");
        });
    }
    
    // Highlight active menu item
    const currentPath = window.location.pathname;
    const menuItems = document.querySelectorAll(".sidebar-menu li a");
    
    menuItems.forEach(function(item) {
        const href = item.getAttribute("href");
        if (href === currentPath || (currentPath === "/" && href === "/Index")) {
            item.classList.add("active");
        }
    });
    
    // Búsqueda con Enter
    const searchInputs = document.querySelectorAll('.search-container input');
    searchInputs.forEach(input => {
        input.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                this.closest('form').submit();
            }
        });
    });
    
    // Custom Select
    setupCustomSelects();
});

function setupCustomSelects() {
    // Encuentra todos los contenedores de select personalizados
    const customSelectContainers = document.querySelectorAll('.custom-select-container');
    
    customSelectContainers.forEach(container => {
        const select = container.querySelector('select');
        const arrow = container.querySelector('.custom-select-arrow');
        
        if (select && arrow) {
            // Evento al hacer clic en el select
            select.addEventListener('click', function(e) {
                e.stopPropagation();
                container.classList.toggle('open');
            });
            
            // Evento al hacer clic en la flecha
            arrow.addEventListener('click', function(e) {
                e.stopPropagation();
                container.classList.toggle('open');
                if (container.classList.contains('open')) {
                    select.focus();
                }
            });
            
            // Evento al cambiar el valor del select
            select.addEventListener('change', function() {
                container.classList.remove('open');
            });
            
            // Cerrar al hacer clic fuera
            document.addEventListener('click', function(e) {
                if (!container.contains(e.target)) {
                    container.classList.remove('open');
                }
            });
            
            // Evento de teclado para accesibilidad
            select.addEventListener('keydown', function(e) {
                if (e.key === 'Escape') {
                    container.classList.remove('open');
                }
            });
        }
    });
    
    // Inicializa los selects con el valor seleccionado
    const selects = document.querySelectorAll('.custom-select, .custom-select-inline');
    selects.forEach(select => {
        updateSelectStyles(select);
        
        select.addEventListener('change', function() {
            updateSelectStyles(this);
        });
    });
}

function updateSelectStyles(select) {
    const selectedOption = select.options[select.selectedIndex];
    if (selectedOption) {
        // Aplicar clase del option seleccionado al select
        const classesToTransfer = ['status-option-pending', 'status-option-in-progress', 
                                  'status-option-completed', 'status-option-cancelled'];
        
        classesToTransfer.forEach(cls => {
            if (selectedOption.classList.contains(cls)) {
                select.classList.add(cls);
            } else {
                select.classList.remove(cls);
            }
        });
    }
}
