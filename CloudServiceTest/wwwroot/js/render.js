﻿
var canvas = document.getElementById('glCanvas');

var gl = canvas.getContext('webgl');

var shaderProgram;

var models = [];
var objects = [];


class Object3D {
    constructor(gl, shaderProgram) {
        this.position = [0, 0, 0];
        this.rotation = [0, 0, 0];
        this.scale = [1, 1, 1];
        this.uWorldMatrixLocation = gl.getUniformLocation(shaderProgram, "uWorldMatrix");
    }


}

var camera = {} ;
camera.position = [0, 0, -2];
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

function initVertices(data) {
    console.log(data);
    let vers = [];
    data.vertices.forEach(function (vertex,index) {
        vers.push(vertex.position.X);
        vers.push(vertex.position.Y);
        vers.push(vertex.position.Z);
        vers.push(vertex.normal.X);
        vers.push(vertex.normal.Y);
        vers.push(vertex.normal.Z);
        vers.push(vertex.color.X);
        vers.push(vertex.color.Y);
        vers.push(vertex.color.Z);
        vers.push(vertex.color.W);
    });

    models.push({ vertices: vers, indices: data.indices });
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

    shaderProgram = createShaderProgram(gl, vsSource, fsSource);
    if (!shaderProgram) {
        console.error('Failed to create shader program.');
        return;
    }
    else {
        console.log("create Shader Program successlly.")
    }

    gl.useProgram(shaderProgram);

    // Continue with WebGL setup and rendering...

    createVertexBuffer(models[0]);
}

function createVertexBuffer(model) {     
    let vertices = model.vertices;
    let indices = model.indices;

    //create vertex buffer
    const vertexBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, vertexBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(vertices), gl.STATIC_DRAW);

    //create index buffer
    const indexBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, indexBuffer);
    gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, new Uint16Array(indices), gl.STATIC_DRAW);

    const positionAttributeLocation = gl.getAttribLocation(shaderProgram, 'aPosition');
    const normalAttributeLocation = gl.getAttribLocation(shaderProgram, 'aNormal');
    const colorAttributeLocation = gl.getAttribLocation(shaderProgram, 'aColor');

    let vertexSize = 10 * Float32Array.BYTES_PER_ELEMENT;


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


    // 设置 color 属性指针
    gl.vertexAttribPointer(
        colorAttributeLocation,
        4,                  //  color's (r, g, b, a)
        gl.FLOAT,           
        false,              // no need for standardization
        vertexSize, // per vertex's lenth
        6 * Float32Array.BYTES_PER_ELEMENT  // offset
    );
    gl.enableVertexAttribArray(colorAttributeLocation);

    //create 3d object 
    const object = new Object3D(gl, shaderProgram);
    objects.push(object);

    // set root parameter
    let worldMatrix = getWorldMatrix(object.position, object.rotation, object.scale);
    gl.uniformMatrix4fv(object.uWorldMatrixLocation, false, worldMatrix);

    const viewMatrix = getViewMatrix(camera); 
    const uViewMatrixLocation = gl.getUniformLocation(shaderProgram, "uViewMatrix");
    gl.uniformMatrix4fv(uViewMatrixLocation, false, viewMatrix);

    const projectionMatrix = getProjectionMatrix();
    const uProjectionMatrixLocation = gl.getUniformLocation(shaderProgram, "uProjectionMatrix");
    gl.uniformMatrix4fv(uProjectionMatrixLocation, false, projectionMatrix);


    gl.clearColor(1.0, 1.0, 0.0, 1.0);

    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);    

    //gl.drawArrays(gl.TRIANGLES, 0, vertices.length / 7);
    gl.drawElements(
        gl.TRIANGLES,      // draw mode: triangle
        indices.length,    
        gl.UNSIGNED_SHORT, 
        0                  // offset
    );

    const error = gl.getError();
    if (error !== gl.NO_ERROR) {
        console.error('WebGL Error:', error);
    }
}

//position, rotation, scale are int arrays, 
function getWorldMatrix(position, rotation, scale) {
    const worldMat = mat4.create();

    const positionRight = leftToRight(position);
    const rotationRight = leftToRight(rotation);

    mat4.translate(worldMat, worldMat, positionRight);

    mat4.rotateZ(worldMat, worldMat, rotationRight[2]);
    mat4.rotateX(worldMat, worldMat, rotationRight[0]);
    mat4.rotateY(worldMat, worldMat, rotationRight[1]);

    mat4.scale(worldMat, worldMat, scale);

    return worldMat;
}

function getViewMatrix(camera) {
    const viewMatrix = mat4.create();
    const position = leftToRight(camera.position);
    const lookAt = leftToRight(camera.lookAt);
    const up = leftToRight(camera.up);
    mat4.lookAt(viewMatrix, position, lookAt, up);

    return viewMatrix;
}

function getProjectionMatrix() {
    const matrix = mat4.create();
    mat4.perspective(matrix, Math.PI / 4, canvas.width / canvas.height, 0.1, 100);

    return matrix;
}

function leftToRight(vector3) {
    return [vector3[0], vector3[1], -vector3[2]];
}

function testRoot(deltaTime) {

    objects.forEach(function (object, index) {
        var rotation = deltaTime * 0.001;
        object.rotation[2] += rotation;

        let newWorldMatrix = getWorldMatrix(object.position, object.rotation, object.scale);
        gl.uniformMatrix4fv(object.uWorldMatrixLocation, false, newWorldMatrix);
    });


    gl.clearColor(1.0, 1.0, 0.0, 1.0);

    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);

    //gl.drawArrays(gl.TRIANGLES, 0, vertices.length / 7);
    gl.drawElements(
        gl.TRIANGLES,      // draw mode: triangle
        6,
        gl.UNSIGNED_SHORT,
        0                  // offset
    );
}

function render() {



}