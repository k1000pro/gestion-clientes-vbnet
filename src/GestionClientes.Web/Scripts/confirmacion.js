// Rellena el modal de confirmación con los datos de la fila cuyo botón lo abrió.
(function () {
    'use strict';

    var modal = document.getElementById('modalEliminar');
    if (!modal) { return; }

    modal.addEventListener('show.bs.modal', function (evento) {
        var boton = evento.relatedTarget;
        if (!boton) { return; }

        var destino = document.getElementById('nombreClienteAEliminar');
        var campo = document.getElementById(modal.getAttribute('data-campo-id'));

        // Se asigna con textContent y no con innerHTML: el nombre lo escribe el usuario, y un
        // cliente llamado "<script>..." se debe ver como texto, no ejecutarse.
        if (destino) {
            destino.textContent = boton.getAttribute('data-cliente-nombre') || '';
        }

        if (campo) {
            campo.value = boton.getAttribute('data-cliente-id') || '';
        }
    });
}());
