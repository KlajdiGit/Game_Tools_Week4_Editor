﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Editor.Engine;

namespace Game_Tools_Week4_Editor.Editor
{
    public class GameEditor : Game
    {
        internal Project Project { get; set; }
        
        private GraphicsDeviceManager m_graphics;
        private SpriteBatch _spriteBatch;
        private FormEditor  m_parent;
        private SpriteBatch m_spriteBatch;
        private FontController m_fonts;
        RasterizerState m_rasterState = new RasterizerState();
        DepthStencilState m_depthStencilState = new DepthStencilState();

        public GameEditor()
        {
            m_graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            m_rasterState = new RasterizerState();
            m_rasterState.CullMode = CullMode.None;
            m_depthStencilState = new DepthStencilState();
            m_depthStencilState.DepthBufferEnable = true;
        }

        public GameEditor(FormEditor _parent) : this()
        {
            m_parent = _parent;
            Form gameForm = Control.FromHandle(Window.Handle) as Form;
            gameForm.TopLevel = false;
            gameForm.Dock = DockStyle.Fill;
            gameForm.FormBorderStyle = FormBorderStyle.None;    
            m_parent.splitContainer.Panel1.Controls.Add(gameForm);
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            //m_camera = new Camera(new Vector3(0, 1, 1), m_graphics.GraphicsDevice.Viewport.AspectRatio);
            RasterizerState state = new RasterizerState();
            state.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = state;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            m_spriteBatch = new(GraphicsDevice);
            m_fonts = new();
            m_fonts.LoadContent(Content);
        }

        protected override void Update(GameTime _gameTime)
        {
            if (Project != null)
            {
                Project.Update((float)(_gameTime.ElapsedGameTime.TotalMilliseconds / 1000));
                InputController.Instance.Clear();
                var models = Project.CurrentLevel.GetSelectedModels();

                if (models.Count == 0)
                {
                    m_parent.propertyGrid.SelectedObject = null;
                }
                else if (models.Count > 1)
                {
                    m_parent.propertyGrid.SelectedObjects = models.ToArray();
                }
                else
                {
                    m_parent.propertyGrid.SelectedObject = models[0];
                }
            }
            base.Update(_gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSeaGreen);

            if(Project != null)
            {
                GraphicsDevice.RasterizerState = m_rasterState;
                GraphicsDevice.DepthStencilState = m_depthStencilState;
            
                Project.Render();
                m_spriteBatch.Begin();
                m_fonts.Draw(m_spriteBatch, 20, InputController.Instance.ToString(), new Vector2(20, 20), Color.White);
                m_fonts.Draw(m_spriteBatch, 16, Project.CurrentLevel.ToString(), new Vector2(20, 80), Color.Yellow);
                m_spriteBatch.End();
            }
        }

        public void AdjustAspectRatio()
        {
            if (Project == null)return;
            Camera c = Project.CurrentLevel.GetCamera();
            c.Viewport = m_graphics.GraphicsDevice.Viewport;
            c.Update(c.Position, m_graphics.GraphicsDevice.Viewport.AspectRatio);
        }
    }
}