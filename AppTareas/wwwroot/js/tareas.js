function agregarNuevaTarea() {
    tareaListadoViewModel.tareas.push(new tareaElementoViewModel({ id: 0, nombre: '' }));

    $("[name=titulo-tarea]").last().focus();
}

async function manejarFocusTitulo(tarea) {
    const nombre = tarea.nombre();

    if (!nombre) {
        tareaListadoViewModel.tareas.pop();
        return;
    }

    const data = JSON.stringify(nombre);
    const respuesta = await fetch(urlTarea, {
        method: 'POST',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (respuesta.ok) {
        const json = await respuesta.json();
        tarea.id(json.id);
    } else {
        manejarError(respuesta);
    }
}

async function obtenerTareas() {
    tareaListadoViewModel.cargando(true);

    const respuesta = await fetch(urlTarea, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (!respuesta.ok) {
        manejarError(respuesta);
        return;
    }

    const json = await respuesta.json();
    tareaListadoViewModel.tareas([]);

    json.forEach(valor => {
        tareaListadoViewModel.tareas.push(new tareaElementoViewModel(valor));
    });

    tareaListadoViewModel.cargando(false);
}

async function actualizarOrden() {
    const ids = obtenerIdTarea();
    await enviarIdsBackend(ids);

    const arregloOrdenado = tareaListadoViewModel.tareas.sorted(function (a, b) {
        return ids.indexOf(a.id().toString()) - ids.indexOf(b.id().toString());
    });

    tareaListadoViewModel.tareas([]);
    tareaListadoViewModel.tareas(arregloOrdenado);
}

function obtenerIdTarea() {
    const ids = $("[name=titulo-tarea]").map(function () {
        return $(this).attr("data-id");
    }).get();

    return ids;
}

async function enviarIdsBackend(ids) {
    var data = JSON.stringify(ids);
    await fetch(`${urlTarea}/ordenar`, {
        method: 'POST',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    });
}

$(function () {
    $("#reordenable").sortable({
        axis: 'y',
        stop: async function () {
            await actualizarOrden();
        }
    })
})

async function clickTarea(tarea) {
    if (tarea.esNuevo()) {
        return;
    }

    const respuesta = await fetch(`${urlTarea}/${tarea.id()}`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (!respuesta.ok) {
        manejarError(respuesta);
        return;
    }

    const json = await respuesta.json();
    
    tareaEditarVM.id = json.id;
    tareaEditarVM.nombre(json.nombre);
    tareaEditarVM.descripcion(json.descripcion);

    tareaEditarVM.pasos([]);

    json.pasos.forEach(paso => {
        tareaEditarVM.pasos.push(
            new pasoViewModel({ ...paso, modoEdit: false })
        )
    })

    modalEditarBootstrap.show();
}

async function CambioEditarTarea() {
    const obj = {
        id: tareaEditarVM.id,
        nombre: tareaEditarVM.nombre(),
        descripcion: tareaEditarVM.descripcion()
    };

    if (!obj.nombre) {
        return;
    }

    await editarTareaCompleta(obj);

    const indice = tareaListadoViewModel.tareas().findIndex(t => t.id() === obj.id);
    const tarea = tareaListadoViewModel.tareas()[indice];
    tarea.nombre(obj.nombre);
}

async function editarTareaCompleta(tarea) {
    const data = JSON.stringify(tarea);

    const respuesta = await fetch(`${urlTarea}/${tarea.id}`, {
        method: 'PUT',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (!respuesta.ok) {
        manejarError(respuesta);
        throw "error";
    }
}

function BorrarTarea(tarea) {
    modalEditarBootstrap.hide();

    confirmarAccion({
        callBackAceptar: () => {
            borrar(tarea);
        },
        callBackCancelar: () => {
            modalEditarBootstrap.show();
        },
        titulo: `¿Desea borrar la tarea ${tarea.nombre()}?`
    })
}

async function borrar(tarea) {
    const idTarea = tarea.id;

    const respuesta = await fetch(`${urlTarea}/${idTarea}`, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (respuesta.ok) {
        const indice = obtenerIndiceTarea();
        tareaListadoViewModel.tareas.splice(indice, 1);
    }
}

function obtenerIndiceTarea() {
    return tareaListadoViewModel.tareas().findIndex(t => t.id() == tareaEditarVM.id);
}

function obtenerTareaEnEdicion() {
    const indice = obtenerIndiceTarea();
    return tareaListadoViewModel.tareas()[indice];
}