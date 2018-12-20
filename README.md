# Leaky Neuron Shader

# Introduction

[Brief demo of shader being used to control a texture](https://youtu.be/Gz2xvLpZeMc)

Simulates a grid of neurons. The neurons spontaneously fire, causing them to propagate activity to adjacent neurons, which causes those adjacent neurons to fire... etc. 

Based on research into neuronal models. See [wikipedia](https://en.wikipedia.org/wiki/Biological_neuron_model) for more information.

# Implementation details

* 512 x 512 grid of neurons at 16 Hz
* neurons have a random uniform distribution of activity growth rates in the range [0-0.08]
* neurons begin firing at 0.9 activity
* neurons leak 50% of their activity when firing
* Regardless of firing state, activity leaks out of the neurons at a rate of 0.1% per tick. To some extent this counterbalances the 0-8% random growth that neurons experience. Neurons also pass activity into other "off the grid" neurons, which effectively means that activity is destroyed

# How to use

Attach ScriptShaderComputeManager to a gameObject with a Renderer which has a texture. Point the ScriptShaderComputeManager's property "cs" to the .compute shader "TestComputeShader.compute" provided in this repository.
