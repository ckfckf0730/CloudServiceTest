import tensorflow as tf
import os

# 设置路径，指向你的 saved_model.pb 的目录
saved_model_dir = 'saved_model'
frozen_graph_dir = 'frozen_graph'

# 加载模型
model = tf.saved_model.load(saved_model_dir)

# 获取模型的默认输入输出签名
infer = model.signatures["serving_default"]

# print("默认张量：", model.signatures['serving_default'])
    
# 查看输入张量的名称、形状和类型
print("=== 输入张量 ===")
for input_name, tensor in infer.structured_input_signature[1].items():
    print(f"名称: {input_name}, 形状: {tensor.shape}, 类型: {tensor.dtype}")

# 查看输出张量的名称、形状和类型
print("=== 输出张量 ===")
for output_name, tensor in infer.structured_outputs.items():
    print(f"名称: {output_name}, 形状: {tensor.shape}, 类型: {tensor.dtype}")
        
# 获取计算图
graph_def = infer.graph.as_graph_def()

# 创建保存冻结图的文件夹（如果不存在）
if not os.path.exists(frozen_graph_dir):
    os.makedirs(frozen_graph_dir)

# 保存冻结图
frozen_graph_filename = os.path.join(frozen_graph_dir, 'frozen_model.pb')

# 将图冻结并保存
with tf.io.gfile.GFile(frozen_graph_filename, "wb") as f:
    f.write(graph_def.SerializeToString())

print(f"冻结图已保存到: {frozen_graph_filename}")