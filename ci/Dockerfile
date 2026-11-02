FROM unityci/editor:ubuntu-6000.3.12f1-linux-il2cpp-3

RUN apt-get update && \
    apt-get install -y blender && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

RUN blender --version
