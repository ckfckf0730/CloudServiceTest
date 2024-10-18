
var canvas = document.getElementById('glCanvas');

var gl = canvas.getContext('webgl');

var shaderProgram;

var vertices = null;

function initVertices(data) {
    vertices = data;
}

if (!gl) {
    console.error("WebGL isn't supported in this browser.");
} else {
    console.log("WebGL is successfully initialized.");
}

gl.clearColor(1.0, 1.0, 0.0, 1.0); 
    
gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);

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

    createVertexBuffer();
}

function createVertexBuffer() { 
    console.error(vertices)
    if (vertices == null) {
        console.error("Failed to load vertices");
        return;
    }

    const vertexBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, vertexBuffer);

    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(vertices), gl.STATIC_DRAW);

    const positionAttributeLocation = gl.getAttribLocation(shaderProgram, 'aPosition');
    const colorAttributeLocation = gl.getAttribLocation(shaderProgram, 'aColor');

    // 设置 position 属性指针
    gl.vertexAttribPointer(
        positionAttributeLocation,
        3,          // 每个顶点有3个值 (x, y, z)
        gl.FLOAT,   // 每个值的类型是32位浮点型
        false,      // 不需要标准化
        7 * Float32Array.BYTES_PER_ELEMENT, // 每个顶点的步长为7个FLOAT（position和color的总大小）
        0           // 数据起点
    );
    gl.enableVertexAttribArray(positionAttributeLocation);

    // 设置 color 属性指针
    gl.vertexAttribPointer(
        colorAttributeLocation,
        4,                  // 每个顶点的 color 部分有4个值 (r, g, b, a)
        gl.FLOAT,           // 每个值的类型是32位浮点型
        false,              // 不需要标准化
        7 * Float32Array.BYTES_PER_ELEMENT, // 每个顶点的步长为7个FLOAT（position和color的总大小）
        3 * Float32Array.BYTES_PER_ELEMENT  // color 数据从第4个FLOAT（position后的第1个位置）开始
    );
    gl.enableVertexAttribArray(colorAttributeLocation);

    gl.drawArrays(gl.TRIANGLES, 0, vertices.length / 3);

    const error = gl.getError();
    if (error !== gl.NO_ERROR) {
        console.error('WebGL Error:', error);
    }
}