namespace Microsoft.Xna.Framework.Graphics
{
	public class RasterizerState : GraphicsResource
	{
        // TODO: We should be asserting if the state has
        // been changed after it has been bound to the device!

        public CullMode CullMode { get; set; }
        public float DepthBias { get; set; }
        public FillMode FillMode { get; set; }
        public bool MultiSampleAntiAlias { get; set; }
        public bool ScissorTestEnable { get; set; }
        public float SlopeScaleDepthBias { get; set; }

		private static readonly Utilities.ObjectFactoryWithReset<RasterizerState> _cullClockwise;
        private static readonly Utilities.ObjectFactoryWithReset<RasterizerState> _cullCounterClockwise;
        private static readonly Utilities.ObjectFactoryWithReset<RasterizerState> _cullNone;

        public static RasterizerState CullClockwise { get { return _cullClockwise.Value; } }
        public static RasterizerState CullCounterClockwise { get { return _cullCounterClockwise.Value; } }
        public static RasterizerState CullNone { get { return _cullNone.Value; } }

        public RasterizerState()
		{
			CullMode = CullMode.CullCounterClockwiseFace;
			FillMode = FillMode.Solid;
			DepthBias = 0;
			MultiSampleAntiAlias = true;
			ScissorTestEnable = false;
			SlopeScaleDepthBias = 0;
		}

		static RasterizerState ()
		{
			_cullClockwise = new Utilities.ObjectFactoryWithReset<RasterizerState>(() => new RasterizerState
            {
                Name = "RasterizerState.CullClockwise",
				CullMode = CullMode.CullClockwiseFace
			});

			_cullCounterClockwise = new Utilities.ObjectFactoryWithReset<RasterizerState>(() => new RasterizerState
            {
                Name = "RasterizerState.CullCounterClockwise",
				CullMode = CullMode.CullCounterClockwiseFace
			});

			_cullNone = new Utilities.ObjectFactoryWithReset<RasterizerState>(() => new RasterizerState
            {
                Name = "RasterizerState.CullNone",
				CullMode = CullMode.None
			});
		}
    }
}
