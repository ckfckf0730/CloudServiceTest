
var canvas = document.getElementById('glCanvas');

var gl = canvas.getContext('webgl');

var shaderProgram;

var models = [];

if (!gl) {
    console.error("WebGL isn't supported in this browser.");
} else {
    console.log("WebGL is successfully initialized.");
}

gl.clearColor(1.0, 1.0, 0.0, 1.0);

gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);

gl.enable(gl.CULL_FACE); // 启用面剔除
gl.cullFace(gl.BACK);

gl.frontFace(gl.CW);

function initVertices(data) {
    console.log(data);
    let vers = [];
    data.vertices.forEach(function (vertex,index) {
        vers.push(vertex.position.X);
        vers.push(vertex.position.Y);
        vers.push(vertex.position.Z);
        vers.push(vertex.color.X);
        vers.push(vertex.color.Y);
        vers.push(vertex.color.Z);
        vers.push(vertex.color.W);
    });
    models.push({ vertices: vers, indices: data.indices });
    console.log(models);
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
    const colorAttributeLocation = gl.getAttribLocation(shaderProgram, 'aColor');

    // 设置 position 属性指针
    gl.vertexAttribPointer(
        positionAttributeLocation,
        3,          // position's (x, y, z)
        gl.FLOAT,   
        false,      // no need for standardization
        7 * Float32Array.BYTES_PER_ELEMENT, // per vertex's lenth
        0           // offset
    );
    gl.enableVertexAttribArray(positionAttributeLocation);

    // 设置 color 属性指针
    gl.vertexAttribPointer(
        colorAttributeLocation,
        4,                  //  color's (r, g, b, a)
        gl.FLOAT,           
        false,              // no need for standardization
        7 * Float32Array.BYTES_PER_ELEMENT, // per vertex's lenth
        3 * Float32Array.BYTES_PER_ELEMENT  // offset
    );
    gl.enableVertexAttribArray(colorAttributeLocation);

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