using ColossalFramework.UI;
using CSUtil.Commons;
using System.Collections.Generic;
using System.Timers;
using TrafficManager.UI.MainMenu;
using UnityEngine;

namespace TMPE_EVE.UI {
    class VehicleButtonExtender : MonoBehaviour {
        private IList<UIButton> buttons;

        public void Start() {
            buttons = new List<UIButton>();

            // Fetch Panels
            var citizenVehicleInfoPanel = GameObject.Find("(Library) CitizenVehicleWorldInfoPanel").GetComponent<CitizenVehicleWorldInfoPanel>();
            var cityServiceVehicleInfoPanel = GameObject.Find("(Library) CityServiceVehicleWorldInfoPanel").GetComponent<CityServiceVehicleWorldInfoPanel>();
            var publicTransportVehicleInfoPanel = GameObject.Find("(Library) PublicTransportVehicleWorldInfoPanel").GetComponent<PublicTransportVehicleWorldInfoPanel>();

            // Add Buttons
            if (citizenVehicleInfoPanel) {
                buttons.Add(AddPullOverButton(citizenVehicleInfoPanel));
            }
            if (cityServiceVehicleInfoPanel) {
                buttons.Add(AddPullOverButton(cityServiceVehicleInfoPanel));
            }
            if (publicTransportVehicleInfoPanel) {
                buttons.Add(AddPullOverButton(publicTransportVehicleInfoPanel));
            }
        }

        public void OnDestroy() {
            if (buttons == null) return;
            foreach (UIButton button in buttons) {
                Destroy(button.gameObject);
            }
        }

        protected UIButton AddPullOverButton(WorldInfoPanel panel) {
            UIButton button = UIView.GetAView().AddUIComponent(typeof(PullOverButton)) as PullOverButton;

            button.AlignTo(panel.component, UIAlignAnchor.TopRight);
            button.relativePosition += new Vector3(- button.width - 115f, 50f);

            return button;
        }

        public class PullOverButton : LinearSpriteButton {
            public override void Start() {
                base.Start();
                width = Width;
                height = Height;
            }

            public override void HandleClick(UIMouseEventParameter p) {
                Log._Debug($"EVE.TextButton.HandleClick: Button clicked!");
                // TODO: Pull over for undefined amount of time, then return.
            }

            public override bool Active {
                get {
                    return false;
                }
            }

            public override Texture2D AtlasTexture {
                get {
                    return TextureResources.PullOverButton2D;
                }
            }

            public override string ButtonName {
                get {
                    return "PullOverButton";
                }
            }

            public override string FunctionName {
                get {
                    return "PullOverButtonNow";
                }
            }

            public override string[] FunctionNames {
                get {
                    return new string[] { "PullOverButtonNow" };
                }
            }

            public override string Tooltip {
                get {
                    return "Tell the vehicle to pull over";
                }
            }

            public override bool Visible {
                get {
                    return true;
                }
            }

            public override int Width {
                get {
                    return 30;
                }
            }

            public override int Height {
                get {
                    return 30;
                }
            }

            public override bool CanActivate() {
                return false;
            }
        }
    }
}
