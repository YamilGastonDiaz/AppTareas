let inputArchivoTarea = document.getElementById('archivoATarea');

function ClickAgregarArchivoAdjunto() {
    inputArchivoTarea.click();
}

async function SeleccionArchivoTarea(event) {
    const archivos = event.target.files;
    const archivosArreglo = Array.from(archivos);

    const idTarea = tareaEditarVM.id;
    const formData = new FormData();
    for (var i = 0; i < archivosArreglo.length; i++) {
        formData.append("archivos", archivosArreglo[i]);
    }

    const respuesta = await fetch(`${urlArchivos}/${idTarea}`, {
        body: formData,
        method: 'POST'
    });

    if (!respuesta.ok) {
        manejarError(respuesta);
        return;
    }

    const json = await respuesta.json();
    prepararArchivosAdjuntos(json);

    inputArchivoTarea.value = null;
}

function prepararArchivosAdjuntos(archivoAdjuntos) {
    archivoAdjuntos.forEach(archivoAdjunto => {
        let fechaCreacion = archivoAdjunto.fechaCreacion;
        if (archivoAdjunto.fechaCreacion.indexOf('Z') === -1) {
            fechaCreacion += 'Z';
        }

        const fechaCreationDT = new Date(fechaCreacion);
        archivoAdjunto.publicado = fechaCreationDT.toLocaleString();

        tareaEditarVM.archivoAdjuntos.push(
            new archivoAdjuntoViewModel({ ...archivoAdjunto, modoEdicion: false }));
    });
}

let tituloArchivoAdjuntoAnterior;
function ClickTituloArchivoAdjunto(archivoAdjunto) {
    archivoAdjunto.modoEdit(true);
    tituloArchivoAdjuntoAnterior = archivoAdjunto.titulo();
    $("[name='txtArchivoAdjuntoTitulo']:visible").focus();
}

async function FocusoutTituloArchivoAdjunto(archivoAdjunto) {
    archivoAdjunto.modoEdit(false);
    const idTarea = archivoAdjunto.id;

    if (!archivoAdjunto.titulo()) {
        archivoAdjunto.titulo(tituloArchivoAdjuntoAnterior);
    }

    if (archivoAdjunto.titulo() === tituloArchivoAdjuntoAnterior) {
        return;
    }

    const data = JSON.stringify(archivoAdjunto.titulo());

    const respuesta = await fetch(`${urlArchivos}/${idTarea}`, {
        body: data,
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (!respuesta.ok) {
        manejarError(respuesta);
    }
}

function ClickBorrarArchivoAdjunto(archivoAdjunto) {
    modalEditarBootstrap.hide();

    confirmarAccion({
        callBackAceptar: () => {
            borrarArchivoAdjunto(archivoAdjunto);
            modalEditarBootstrap.show();
        },
        callBackCancelar: () => {
            modalEditarBootstrap.show();
        },
        titulo: '¿Desea borrar este archivo adjunto?'
    });
}

async function borrarArchivoAdjunto(archivoAdjunto) {
    const respuesta = await fetch(`${urlArchivos}/${archivoAdjunto.id}`, {
        method: 'DELETE'
    });

    if (!respuesta.ok) {
        manejarError(respuesta);
        return;
    }

    tareaEditarVM.archivoAdjuntos.remove(function (item) { return item.id == archivoAdjunto.id });
}

function ClickDescargarArchivoAdjunto(archivoAdjunto) {
    descargarArchivo(archivoAdjunto.url, archivoAdjunto.titulo());
}

$(function () {
    $("#reordenable-adjuntos").sortable({
        axis: 'y',
        stop: async function () {
            await actualizarOrdenArchivos();
        }
    })
})

async function actualizarOrdenArchivos() {
    const ids = obtenerIdsArchivos();
    await enviarIdsArchivosAlBackend(ids);

    const arregloOrganizado = tareaEditarVM.archivoAdjuntos.sorted(function (a, b) {
        return ids.indexOf(a.id.toString()) - ids.indexOf(b.id.toString());
    })

    tareaEditarVM.archivoAdjuntos(arregloOrganizado);
}

function obtenerIdsArchivos() {
    const ids = $("[name=txtArchivoAdjuntoTitulo]").map(function () {
        return $(this).attr('data-id')
    }).get();
    return ids;
}

async function enviarIdsArchivosAlBackend(ids) {
    var data = JSON.stringify(ids);
    const respuesta = await fetch(`${urlArchivos}/ordenar/${tareaEditarVM.id}`, {
        method: 'POST',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (!respuesta.ok) {
        manejarError(respuesta);
    }
}