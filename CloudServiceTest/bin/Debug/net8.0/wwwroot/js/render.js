
var canvas = document.getElementById('glCanvas');

var gl = canvas.getContext('webgl');

var shaderProgram;

var modelMap = new Map();
var textureMap = new Map();
var waitResourceObjects = new Map();
var objects = [];
//var testIndex = 0;

var frameObjects = [];
var curFrameIndex = 0


var requestResource = null;

class Object3D {
    constructor(gl, shaderProgram) {
        this.position = [0, 0, 0];
        this.rotation = [0, 0, 0];
        this.scale = [1, 1, 1];
        this.color = [1, 1, 1, 1];
        this.uWorldMatrixLocation = gl.getUniformLocation(shaderProgram, "uWorldMatrix");
        this.uColorLocation = gl.getUniformLocation(shaderProgram, "uColor");

        this.name = null;
        this.model = null;
        this.texture = null;
    }


}

var camera = {};
camera.position = [0, 3, -4];
camera.rotation = [0.6, 0, 0];
camera.lookAt = [0, 0, 0];
camera.up = [0, 1, 0];

if (!gl) {
    console.error("WebGL isn't supported in this browser.");
} else {
    console.log("WebGL is successfully initialized.");
}

gl.clearColor(1.0, 1.0, 0.0, 1.0);

gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);

gl.enable(gl.CULL_FACE);
gl.cullFace(gl.BACK);

gl.frontFace(gl.CW);

gl.disable(gl.BLEND);

gl.enable(gl.DEPTH_TEST);
gl.depthFunc(gl.LESS);


function createTexture(name, picture) {
    const image = new Image();
    image.src = picture;
    image.onload = function () {
        initTexture(name, image);
    };
}

function initTexture(name, image) {
    let texture = gl.createTexture();
    gl.bindTexture(gl.TEXTURE_2D, texture);

    gl.texImage2D(
        gl.TEXTURE_2D,
        0,
        gl.RGBA,
        gl.RGBA,
        gl.UNSIGNED_BYTE,
        image
    );

    // 设置纹理参数
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);

    // 解绑纹理
    gl.bindTexture(gl.TEXTURE_2D, null);

    textureMap.set(name, texture);

    if (waitResourceObjects.has(name)) {
        waitResourceObjects.get(name).forEach(function (object, index) {
            object.texture = texture;
        });

        waitResourceObjects.delete(name);
    }
}

function createObject3D(objectData) {
    //create 3d object 
    const object = new Object3D(gl, shaderProgram);

    object.position[0] = objectData.position.X;
    object.position[1] = objectData.position.Y;
    object.position[2] = objectData.position.Z;
    object.rotation[0] = objectData.rotation.X;
    object.rotation[1] = objectData.rotation.Y;
    object.rotation[2] = objectData.rotation.Z;
    object.scale[0] = objectData.scale.X;
    object.scale[1] = objectData.scale.Y;
    object.scale[2] = objectData.scale.Z;

    object.color[0] = objectData.color.X;
    object.color[1] = objectData.color.Y;
    object.color[2] = objectData.color.Z;
    object.color[3] = objectData.color.W;

    if (objectData.remark == "Frame") {
        object.color = [1, 1, 1, 1];
        objectData.texture = "frame_" + frameObjects.length;
        frameObjects.push(object);
    }

    object.name = objectData.name;

    if (modelMap.has(objectData.model)) {
        object.model = modelMap.get(objectData.model);
    }
    else {
        console.log("Model " + objectData.model + " not exist.");
        requestResource("CreateModel", objectData.model);
        if (waitResourceObjects.has(objectData.model)) {
            waitResourceObjects.get(objectData.model).push(object);
        }
        else {
            const waitObjs = [];
            waitObjs.push(object);
            waitResourceObjects.set(objectData.model, waitObjs);
        }


    }

    if (textureMap.has(objectData.texture)) {
        object.texture = textureMap.get(objectData.texture);
    }
    else {
        console.log("Texture " + objectData.texture + " not exist.");
        requestResource("CreateTexture", objectData.texture);
        if (waitResourceObjects.has(objectData.texture)) {
            waitResourceObjects.get(objectData.texture).push(object);
        }
        else {
            const waitObjs = [];
            waitObjs.push(object);
            waitResourceObjects.set(objectData.texture, waitObjs);
        }
    }

    objects.push(object);

    let worldMatrix = getWorldMatrix(object.position, object.rotation, object.scale);
    gl.uniformMatrix4fv(object.uWorldMatrixLocation, false, worldMatrix);

    gl.uniform4fv(object.uColorLocation, object.color);
}

async function loadShaderFile(url) {
    try {
        const response = await fetch(url);
        if (!response.ok) {
            throw new Error(`Failed to load shader file: ${url}`);
        }
        return await response.text();
    } catch (error) {
        console.error(error);
        return null;
    }
}

function loadShader(gl, type, source) {
    const shader = gl.createShader(type);
    gl.shaderSource(shader, source);
    gl.compileShader(shader);

    // 检查编译结果
    if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
        console.error(`An error occurred compiling the shaders: ${gl.getShaderInfoLog(shader)}`);
        gl.deleteShader(shader);
        return null;
    }

    return shader;
}

function createShaderProgram(gl, vsSource, fsSource) {
    const vertexShader = loadShader(gl, gl.VERTEX_SHADER, vsSource);
    const fragmentShader = loadShader(gl, gl.FRAGMENT_SHADER, fsSource);

    const shaderProgram = gl.createProgram();
    gl.attachShader(shaderProgram, vertexShader);
    gl.attachShader(shaderProgram, fragmentShader);
    gl.linkProgram(shaderProgram);

    if (!gl.getProgramParameter(shaderProgram, gl.LINK_STATUS)) {
        console.error(`Unable to initialize the shader program: ${gl.getProgramInfoLog(shaderProgram)}`);
        return null;
    }

    return shaderProgram;
}

async function initWebGL() {
    const vsSource = await loadShaderFile('/glsl/vsBasic.glsl');
    const fsSource = await loadShaderFile('/glsl/psBasic.glsl');

    if (!vsSource || !fsSource) {
        console.error('Failed to load shaders.');
        return;
    }
    else {
        console.log("load shaders successlly.")
    }

    shaderProgram = await createShaderProgram(gl, vsSource, fsSource);
    if (!shaderProgram) {
        console.error('Failed to create shader program.');
        return;
    }
    else {
        console.log("create Shader Program successlly.")
    }

    gl.useProgram(shaderProgram);


    // set root parameter

    const viewMatrix = getViewMatrix(camera);
    const uViewMatrixLocation = gl.getUniformLocation(shaderProgram, "uViewMatrix");
    gl.uniformMatrix4fv(uViewMatrixLocation, false, viewMatrix);

    const projectionMatrix = getProjectionMatrix();
    const uProjectionMatrixLocation = gl.getUniformLocation(shaderProgram, "uProjectionMatrix");
    gl.uniformMatrix4fv(uProjectionMatrixLocation, false, projectionMatrix);

    const eye = leftToRight(camera.position);
    const uEyeLocation = gl.getUniformLocation(shaderProgram, "uEye");
    gl.uniform3fv(uEyeLocation, eye);
}

function createVertexBuffer(name, data) {

    let vers = [];
    data.vertices.forEach(function (vertex, index) {
        vers.push(vertex.position.X);
        vers.push(vertex.position.Y);
        vers.push(-vertex.position.Z);
        vers.push(vertex.normal.X);
        vers.push(vertex.normal.Y);
        vers.push(-vertex.normal.Z);
        vers.push(vertex.uv.X);
        vers.push(vertex.uv.Y);
    });

    let model = { vertices: vers, indices: data.indices, vBuffer: null, iBuffer: null };

    //create vertex buffer
    model.vBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, model.vBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(vers), gl.STATIC_DRAW);

    const positionAttributeLocation = gl.getAttribLocation(shaderProgram, 'aPosition');
    const normalAttributeLocation = gl.getAttribLocation(shaderProgram, 'aNormal');
    const uvAttributeLocation = gl.getAttribLocation(shaderProgram, 'aUv');

    let vertexSize = 8 * Float32Array.BYTES_PER_ELEMENT;

    // 设置 position 属性指针
    gl.vertexAttribPointer(
        positionAttributeLocation,
        3,          // position's (x, y, z) 
        gl.FLOAT,
        false,      // no need for standardization
        vertexSize, // per vertex's lenth
        0           // offset
    );
    gl.enableVertexAttribArray(positionAttributeLocation);

    // 设置 normal 属性指针
    gl.vertexAttribPointer(
        normalAttributeLocation,
        3,          // normal's (x, y, z)
        gl.FLOAT,
        false,      // no need for standardization
        vertexSize, // per vertex's lenth
        3 * Float32Array.BYTES_PER_ELEMENT          // offset
    );
    gl.enableVertexAttribArray(normalAttributeLocation);

    // 设置 uv 属性指针
    gl.vertexAttribPointer(
        uvAttributeLocation,
        2,                  //  uv's (x, y)
        gl.FLOAT,
        false,              // no need for standardization
        vertexSize, // per vertex's lenth
        6 * Float32Array.BYTES_PER_ELEMENT  // offset
    );
    gl.enableVertexAttribArray(uvAttributeLocation);

    //gl.bindBuffer(gl.ARRAY_BUFFER, null);

    //create index buffer
    model.iBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, model.iBuffer);
    gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, new Uint16Array(data.indices), gl.STATIC_DRAW);
    //gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, null);

    modelMap.set(name, model);

    if (waitResourceObjects.has(name)) {
        waitResourceObjects.get(name).forEach(function (object, index) {
            object.model = model;
        });

        waitResourceObjects.delete(name);
    }
}

function ResetVertexAttribPooint() {
    const positionAttributeLocation = gl.getAttribLocation(shaderProgram, 'aPosition');
    const normalAttributeLocation = gl.getAttribLocation(shaderProgram, 'aNormal');
    const uvAttributeLocation = gl.getAttribLocation(shaderProgram, 'aUv');

    let vertexSize = 8 * Float32Array.BYTES_PER_ELEMENT;
    gl.vertexAttribPointer(positionAttributeLocation, 3, gl.FLOAT, false, vertexSize, 0);
    gl.enableVertexAttribArray(positionAttributeLocation);
    gl.vertexAttribPointer(normalAttributeLocation, 3, gl.FLOAT, false, vertexSize, 3 * Float32Array.BYTES_PER_ELEMENT);
    gl.enableVertexAttribArray(normalAttributeLocation);
    gl.vertexAttribPointer(uvAttributeLocation, 2, gl.FLOAT, false, vertexSize, 6 * Float32Array.BYTES_PER_ELEMENT);
    gl.enableVertexAttribArray(uvAttributeLocation);
}

//position, rotation, scale are int arrays, 
function getWorldMatrix(position, rotation, scale) {
    const worldMat = mat4.create();

    const positionRight = leftToRight(position);
    let rotationRight = leftToRight(rotation);
    rotationRight = float3Reflect(rotationRight);

    mat4.translate(worldMat, worldMat, positionRight);

    mat4.rotateY(worldMat, worldMat, rotationRight[1]);
    mat4.rotateX(worldMat, worldMat, rotationRight[0]);
    mat4.rotateZ(worldMat, worldMat, rotationRight[2]);

    mat4.scale(worldMat, worldMat, scale);

    return worldMat;
}

function getViewMatrix(camera) {
    const viewMatrix = mat4.create();
    const position = leftToRight(camera.position);
    const rotationMat = mat4.create();

    let rotationRight = leftToRight(camera.rotation);
    rotationRight = float3Reflect(rotationRight);

    mat4.rotateY(rotationMat, rotationMat, rotationRight[1]);
    mat4.rotateX(rotationMat, rotationMat, rotationRight[0]);
    mat4.rotateZ(rotationMat, rotationMat, rotationRight[2]);

    const lookAt = [0, 0, -1];
    const result = vec3.create();
    vec3.transformMat4(result, lookAt, rotationMat);
    vec3.add(result, result, position);

    const up = leftToRight(camera.up);
    mat4.lookAt(viewMatrix, position, result, up);

    return viewMatrix;
}

function cameraMove(camera, offset) {
    const rotationMat = mat4.create();

    let rotationRight = leftToRight(camera.rotation);
    rotationRight = float3Reflect(rotationRight);

    mat4.rotateY(rotationMat, rotationMat, rotationRight[1]);
    mat4.rotateX(rotationMat, rotationMat, rotationRight[0]);
    mat4.rotateZ(rotationMat, rotationMat, rotationRight[2]);

    const result = vec3.create();

    const offsetRight = [offset[0], offset[1], -offset[2]]
    vec3.transformMat4(result, offsetRight, rotationMat);

    camera.position[0] += result[0];
    camera.position[1] += result[1];
    camera.position[2] += -result[2];
}

function getProjectionMatrix() {
    const matrix = mat4.create();
    mat4.perspective(matrix, Math.PI / 4, canvas.width / canvas.height, 0.1, 100);

    return matrix;
}

function leftToRight(vector3) {
    return [vector3[0], vector3[1], -vector3[2]];
}

function float3Reflect(vector3) {
    return [-vector3[0], -vector3[1], -vector3[2]];
}

function testRoot(deltaTime) {
    UpdateCamera();

    gl.clearColor(1.0, 1.0, 0.0, 1.0);

    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);

    objects.forEach(function (object, index) {
        if (object.model == null) {
            return;
        }

        //var rotation = deltaTime * 0.001;
        //object.rotation[1] += rotation;

        let newWorldMatrix = getWorldMatrix(object.position, object.rotation, object.scale);
        gl.uniformMatrix4fv(object.uWorldMatrixLocation, false, newWorldMatrix);

        gl.uniform4fv(object.uColorLocation, object.color);

        gl.bindBuffer(gl.ARRAY_BUFFER, object.model.vBuffer)
        ResetVertexAttribPooint();
        gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, object.model.iBuffer);
        gl.bindTexture(gl.TEXTURE_2D, object.texture);
        gl.activeTexture(gl.TEXTURE0); // 激活纹理单元 0
        gl.uniform1i(gl.getUniformLocation(shaderProgram, 'uSampler'), 0);

        //gl.drawArrays(gl.TRIANGLES, 0, vertices.length / 7);
        gl.drawElements(
            gl.TRIANGLES,      // draw mode: triangle
            object.model.indices.length,
            gl.UNSIGNED_SHORT,
            0                  // offset
        );
    });


}

function UpdateCamera() {
    const viewMatrix = getViewMatrix(camera);
    const uViewMatrixLocation = gl.getUniformLocation(shaderProgram, "uViewMatrix");
    gl.uniformMatrix4fv(uViewMatrixLocation, false, viewMatrix);

    const eye = leftToRight(camera.position);
    const uEyeLocation = gl.getUniformLocation(shaderProgram, "uEye");
    gl.uniform3fv(uEyeLocation, eye);
}

function render() {



}


window.addEventListener('keydown', (event) => {
    const moveSpeed = 0.1;

    if (event.code === 'KeyW') {
        const moveVec = [0, 0, moveSpeed];
        cameraMove(camera, moveVec);

    } else if (event.code === 'KeyS') {
        const moveVec = [0, 0, -moveSpeed];
        cameraMove(camera, moveVec);
    } else if (event.code === 'KeyA') {
        const moveVec = [-moveSpeed, 0, 0];
        cameraMove(camera, moveVec);
    } else if (event.code === 'KeyD') {
        const moveVec = [moveSpeed, 0, 0];
        cameraMove(camera, moveVec);
    }
});

let lastX = 0, lastY = 0;
let isMouseDown = false;
let rotationSpeed = 0.001;

window.addEventListener('mousedown', (event) => {
    isMouseDown = true;
    lastX = event.clientX;
    lastY = event.clientY;
});

window.addEventListener('mouseup', () => {
    isMouseDown = false;
});

window.addEventListener('mousemove', (event) => {
    if (isMouseDown) {
        let deltaX = event.clientX - lastX;
        let deltaY = event.clientY - lastY;


        camera.rotation[1] += deltaX * rotationSpeed;
        camera.rotation[0] += deltaY * rotationSpeed;


        lastX = event.clientX;
        lastY = event.clientY;
    }
});


function showAzurePicture(texture) {
    if (curFrameIndex < frameObjects.length) {
        let name = "frame_" + curFrameIndex;
        createTexture(name, texture);
        curFrameIndex++;
    }

}