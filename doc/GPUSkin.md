## 概要

GPUSkin可以通过离线烘焙动作数据到顶点数据贴图（以下简称AnimationMap）的方式，在运行时避免大量的Animation计算（多层骨骼的矩阵运算），缓解CPU的压力。应用场景为简单动画的多个实例同时显示，如部队、草地、旗帜等。参考资料有：

- [GPUInstancing](https://docs.unity3d.com/Manual/GPUInstancing.html)
- [github上的一个GPUSkin的解决方案](https://github.com/chenjd/Render-Crowd-Of-Animated-Characters)
- [模型资源](https://assetstore.unity.com/packages/3d/characters/humanoids/mini-legion-footman-hp-pbr-86576)

实践中我们发现，可以对上述解决方案做如下扩展：

- 将一个模型的多个动作烘焙到一张AnimationMap上，并将各动作的区段信息（起始帧和结束帧）保存到prefab中，运行时可以通过函数来切换动作。
- 多个实例播放同一个动作，也希望不要那么整齐。引入偏移帧可以解决这个问题，偏移帧的范围自然就在起始帧和结束帧之间。
- 要能加速，不依赖Application.timeScale的加速。
- 使用MaterialPropertyBlock封装以上提到的参数，就可以很好的使用GPUInstancing了。

## 限制

- 移动平台贴图尺寸不宜超过1024，而贴图的横坐标对应的是顶点数，故待烘焙的模型顶点数不能超过1024。
- 贴图的纵坐标为所有动画帧数的总和，也不能超过1024，但大部分简单的动画基本满足这个设定。
- 本例提供将CPU的计算压力平衡到GPU的方法，如果实际项目中GPU的渲染压力本来就大，就需要慎重选择使用。