﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Editor.Engine.Interfaces;
using Microsoft.Xna.Framework.Content;
using System.IO;
using Game_Tools_Week4_Editor;
using Editor.Engine;
using Game_Tools_Week4_Editor.Engine.Interfaces;
using Game_Tools_Week4_Editor.Editor;

namespace Game_Tools_Week4_Editor
{
    class Models : ISerializable, ISelectable
    {
        // Accessors
        public Model Mesh { get; set; }
        public Effect Shader { get; set; }
        public Vector3 Position { get => m_position; set { m_position = value; } }
        public Vector3 Rotation { get => m_rotation; set { m_rotation = value; } }
        public float Scale { get; set; }
        public bool Selected
        { 
            get { return m_selected; } 
            set
            {
                if(m_selected != value)
                {
                    m_selected = value;
                    SelectedDirty = true;
                }
            }
        }
        public static bool SelectedDirty { get; set; } = false;


        // Texturing
        public Texture Texture { get; set; }

        //Members
        private Vector3 m_position;
        private Vector3 m_rotation;
        private bool m_selected;

        public Models()
        {
        }

        public Models( GameEditor _game,
                       string _model, 
                       string _texture,
                       string _effect,
                       Vector3 _position,
                       float _scale)
        {
            Create(_game, _model, _texture, _effect, _position, _scale);
        }

        public void Create(GameEditor _game,
                       string _model,
                       string _texture,
                       string _effect,
                       Vector3 _position,
                       float _scale)
        {
            Mesh = _game.Content.Load<Model>( _model );
            Mesh.Tag = _model;
            if(_texture == "DefaultTexture")
            {
                Texture = _game.DefaultTexture;
            }
            else
            {
                Texture = _game.Content.Load<Texture>( _texture );
            }
            Texture.Tag = _texture;
            if(_effect == "DefaultEffect")
            {
                Shader = _game.DefaultEffect;
            }
            else
            {
                Shader = _game.Content.Load<Effect>( _effect );
            }
            Shader.Tag = _effect;
            SetShader(Shader);
            m_position = _position;
            Scale = _scale;
        }

        public void SetShader(Effect _effect)
        {
            Shader = _effect;
            foreach(ModelMesh mesh in Mesh.Meshes)
            {
                foreach(ModelMeshPart meshPart in mesh.MeshParts) 
                {
                    meshPart.Effect = Shader;
                }
            }
        }

        public void Translate(Vector3 _translate, Camera _camera)
        {
            float distance = Vector3.Distance(_camera.Target, _camera.Position);
            Vector3 forward = _camera.Target - _camera.Position;
            forward.Normalize();
            Vector3 left = Vector3.Cross(forward, Vector3.Up);
            left.Normalize();
            Vector3 up = Vector3.Cross(left, forward);
            up.Normalize();
            Position += left * _translate.X * distance;
            Position += up * _translate.Y * distance;
            Position += forward * _translate.Z * 100f;
        }

        public void Rotate(Vector3 _rotate)
        {
            Rotation += _rotate;
        }

        public Matrix GetTransform()
        {
            return Matrix.CreateScale(Scale) *
                   Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) *
                   Matrix.CreateTranslation(Position);
        }

        public void Render(Matrix _view, Matrix _projection)
        {
            // m_rotation.X += 0.001f;
            // m_rotation.Y += 0.005f;

            Shader.Parameters["World"].SetValue(GetTransform());
            Shader.Parameters["WorldViewProjection"].SetValue(GetTransform() * _view * _projection);
            Shader.Parameters["Texture"].SetValue(Texture);
            Shader.Parameters["Tint"].SetValue(Selected);

            foreach(ModelMesh mesh in Mesh.Meshes)
            {
                mesh.Draw();
            }
        }

        public void Serialize(BinaryWriter _stream)
        {
            _stream.Write(Mesh.Tag.ToString());
            _stream.Write(Texture.Tag.ToString());
            _stream.Write(Shader.Tag.ToString());
            HelpSerialize.Vec3(_stream, Position);
            HelpSerialize.Vec3(_stream, Rotation);
            _stream.Write(Scale);
        }

        public void Deserialize(BinaryReader _stream, GameEditor _game)
        {
            string mesh = _stream.ReadString();
            string texture = _stream.ReadString();
            string shader = _stream.ReadString();
            Position = HelpDeserialize.Vec3(_stream);
            Rotation = HelpDeserialize.Vec3(_stream);
            Scale = _stream.ReadSingle();
            Create(_game, mesh, texture, shader, Position, Scale);
        }
    }
}
