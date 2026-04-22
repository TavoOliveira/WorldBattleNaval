using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldBattleNaval;

namespace WorldBattleNaval.Entities;

public class ParticleSystem
{
    private const int POOL_SIZE = 2048;
    private const float EMIT_INTERVAL = 0.05f;

    private struct Particle
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public float Life;
        public float MaxLife;
        public float Size;
    }

    private class Emitter
    {
        public Vector3 Origin;
        public float Accumulator;
    }

    private readonly Particle[] particles = new Particle[POOL_SIZE];
    private int activeCount;

    private readonly VertexPositionColor[] vertexBuffer = new VertexPositionColor[POOL_SIZE * 4];
    private readonly short[] indexBuffer;

    private readonly List<Emitter> emitters = [];
    private readonly BasicEffect effect;
    private readonly Random rand = new();

    public ParticleSystem(GraphicsDevice graphicsDevice)
    {
        effect = new BasicEffect(graphicsDevice)
        {
            VertexColorEnabled = true,
            LightingEnabled = false,
            TextureEnabled = false
        };

        indexBuffer = new short[POOL_SIZE * 6];
        for (int i = 0; i < POOL_SIZE; i++)
        {
            int v = i * 4;
            int idx = i * 6;
            indexBuffer[idx + 0] = (short)(v + 0);
            indexBuffer[idx + 1] = (short)(v + 1);
            indexBuffer[idx + 2] = (short)(v + 2);
            indexBuffer[idx + 3] = (short)(v + 0);
            indexBuffer[idx + 4] = (short)(v + 2);
            indexBuffer[idx + 5] = (short)(v + 3);
        }
    }

    public void AddEmitter(Vector3 origin)
    {
        emitters.Add(new Emitter { Origin = origin });
    }

    public void Update(float dt)
    {
        for (int i = activeCount - 1; i >= 0; i--)
        {
            ref var p = ref particles[i];
            p.Life -= dt;
            if (p.Life <= 0f)
            {
                particles[i] = particles[activeCount - 1];
                activeCount--;
                continue;
            }
            p.Position += p.Velocity * dt;
        }

        foreach (var e in emitters)
        {
            e.Accumulator += dt;
            while (e.Accumulator >= EMIT_INTERVAL && activeCount < POOL_SIZE)
            {
                e.Accumulator -= EMIT_INTERVAL;
                SpawnParticle(e.Origin);
            }
        }
    }

    private void SpawnParticle(Vector3 origin)
    {
        float jitterX = ((float)rand.NextDouble() - 0.5f) * 0.4f;
        float jitterZ = ((float)rand.NextDouble() - 0.5f) * 0.4f;
        float life = 1.4f + (float)rand.NextDouble() * 0.9f;

        particles[activeCount++] = new Particle
        {
            Position = origin + new Vector3(jitterX * 0.5f, 0.1f, jitterZ * 0.5f),
            Velocity = new Vector3(jitterX, 1.1f + (float)rand.NextDouble() * 0.8f, jitterZ),
            Life = life,
            MaxLife = life,
            Size = 0.4f + (float)rand.NextDouble() * 0.25f
        };
    }

    public void Draw(GraphicsDevice graphicsDevice, Camera camera)
    {
        if (activeCount == 0) return;

        var view = camera.View;
        var projection = camera.Projection;
        var camRight = camera.Right;
        var camUp = camera.Up;

        for (int i = 0; i < activeCount; i++)
        {
            ref var p = ref particles[i];
            float ageT = 1f - MathHelper.Clamp(p.Life / p.MaxLife, 0f, 1f);
            float alpha = 1f - ageT;
            float size = p.Size * (0.7f + ageT * 0.8f);
            var color = new Color(110, 110, 110) * alpha;

            var r = camRight * (size * 0.5f);
            var u = camUp * (size * 0.5f);

            int v = i * 4;
            vertexBuffer[v + 0] = new VertexPositionColor(p.Position - r - u, color);
            vertexBuffer[v + 1] = new VertexPositionColor(p.Position + r - u, color);
            vertexBuffer[v + 2] = new VertexPositionColor(p.Position + r + u, color);
            vertexBuffer[v + 3] = new VertexPositionColor(p.Position - r + u, color);
        }

        graphicsDevice.BlendState = BlendState.AlphaBlend;
        graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
        graphicsDevice.RasterizerState = RasterizerState.CullNone;

        effect.View = view;
        effect.Projection = projection;
        effect.World = Matrix.Identity;
        effect.CurrentTechnique.Passes[0].Apply();

        graphicsDevice.DrawUserIndexedPrimitives(
            PrimitiveType.TriangleList,
            vertexBuffer, 0, activeCount * 4,
            indexBuffer, 0, activeCount * 2);

        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
    }
}
