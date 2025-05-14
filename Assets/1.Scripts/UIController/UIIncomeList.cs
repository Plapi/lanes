using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using frame8.Logic.Misc.Other.Extensions;
using Com.ForbiddenByte.OSA.Core;
using Com.ForbiddenByte.OSA.CustomParams;
using Com.ForbiddenByte.OSA.DataHelpers;
using TMPro;

public class UIIncomeList : OSA<BaseParamsWithPrefab, UIIncomeList.IncomeListItemViewsHolder> {

	private SimpleDataHelper<IncomeListData> data;
	private readonly List<float> sizes = new();

	public new void Init() {
		data = new SimpleDataHelper<IncomeListData>(this);
		sizes.Clear();

		PlayerPrefsManager.UserData.TryGetCoinsIncome(out int profitPerTurn);
		data.List.Add(new IncomeListData {
			type = IncomeListType.Vault,
			vaultListData = new VaultListData {
				coins = PlayerPrefsManager.UserData.coins,
				profitPerTurn = profitPerTurn,
				storage = PlayerPrefsManager.UserData.CalculateCapacity()
			}
		});
		sizes.Add(320f);

		FloorData[] floorData = PlayerPrefsManager.UserData.floors;
		profitPerTurn = 0;
		for (int i = 0; i < floorData.Length; i++) {
			profitPerTurn += floorData[i].waitingRoom.CoinsIncome + floorData[i].callCenterRoom.CoinsIncome + floorData[i].breakRoom.CoinsIncome;
		}
		data.List.Add(new IncomeListData {
			type = IncomeListType.Top,
			topListData = new TopListData {
				title = "Building",
				profitPerTurn = profitPerTurn
			}
		});
		sizes.Add(80f);
		
		for (int i = 0; i < floorData.Length; i++) {
			profitPerTurn = floorData[i].waitingRoom.CoinsIncome + floorData[i].callCenterRoom.CoinsIncome + floorData[i].breakRoom.CoinsIncome;
			data.List.Add(new IncomeListData {
        		type = IncomeListType.Floor,
		        florListData = new FloorListData {
			        level = i,
			        profitPerTurn = profitPerTurn,
			        waitingRoom = floorData[i].waitingRoom,
			        callCenterRoom = floorData[i].callCenterRoom,
			        breakRoom = floorData[i].breakRoom,
			        isLast = i == floorData.Length - 1,
		        }
        	});
			sizes.Add(320f);
		}

		ParkingSlotData[] parkingSlots = PlayerPrefsManager.UserData.parkingRoom.parkingSlots;
		List<ParkingSlotData> slotsWithDrivers = new();
		profitPerTurn = 0;
		for (int i = 0; i < parkingSlots.Length; i++) {
			if (parkingSlots[i].HasDriver) {
				slotsWithDrivers.Add(parkingSlots[i]);
				profitPerTurn += PlayerPrefsManager.UserData.GetDriver(slotsWithDrivers[i].driverId).design.income;
			}
		}
		if (slotsWithDrivers.Count > 0) {
			data.List.Add(new IncomeListData { type = IncomeListType.Empty });
			sizes.Add(50f);
			data.List.Add(new IncomeListData {
				type = IncomeListType.Top,
				topListData = new TopListData {
					title = "Drivers",
					profitPerTurn = profitPerTurn
				}
			});
			sizes.Add(100f);
			for (int i = 0; i < slotsWithDrivers.Count; i++) {
				data.List.Add(new IncomeListData {
					type = IncomeListType.Drivers,
					driverListData = new DriverListData {
						driver = PlayerPrefsManager.UserData.GetDriver(slotsWithDrivers[i].driverId),
						isLast = i == slotsWithDrivers.Count - 1
					}
				});
				sizes.Add(i == slotsWithDrivers.Count - 1 ? 100f : 80f);
			}
		}
		
		_Params.ItemPrefab.gameObject.SetActive(false);
		data.NotifyListChangedExternally();
		ScrollTo(0, 0f, -_Params.ContentPadding.top);
	}

	public void OnCoinsUpdate() {
		data.List[0] = GetVaultListData();
		data.NotifyListChangedExternally();
	}

	private IncomeListData GetVaultListData() {
		PlayerPrefsManager.UserData.TryGetCoinsIncome(out int profitPerTurn);
		return new IncomeListData {
			type = IncomeListType.Vault,
			vaultListData = new VaultListData {
				coins = PlayerPrefsManager.UserData.coins,
				profitPerTurn = profitPerTurn,
				storage = PlayerPrefsManager.UserData.CalculateCapacity()
			}
		};
	}

	protected override IncomeListItemViewsHolder CreateViewsHolder(int itemIndex) {
		IncomeListItemViewsHolder instance = new IncomeListItemViewsHolder();
		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
		return instance;
	}
	
	protected override void UpdateViewsHolder(IncomeListItemViewsHolder newOrRecycled) {
		newOrRecycled.Init(data[newOrRecycled.ItemIndex]);
	}

	protected override void CollectItemsSizes(ItemCountChangeMode changeMode, int count, int indexIfInsertingOrRemoving, ItemsDescriptor itemsDesc) {
		base.CollectItemsSizes(changeMode, count, indexIfInsertingOrRemoving, itemsDesc);
		if (data != null) {
			itemsDesc.BeginChangingItemsSizes(0);
			if (sizes.Count == itemsDesc.itemsCount) {
				for (int i = 0; i < sizes.Count; i++) {
					itemsDesc[i] = sizes[i];
				}
			}
			itemsDesc.EndChangingItemsSizes();
		}
	}

	public class IncomeListData {
		public IncomeListType type;
		public VaultListData vaultListData;
		public TopListData topListData;
		public FloorListData florListData;
		public DriverListData driverListData;
	}

	public enum IncomeListType {
		Vault,
		Top,
		Floor,
		Drivers,
		Empty
	}
	
	public class IncomeListItemViewsHolder : BaseItemViewsHolder {

		public void Init(IncomeListData data) {
			if (data.type == IncomeListType.Empty) {
				foreach (Transform t in root.transform) {
					t.gameObject.SetActive(false);
				}
				return;
			}
			root.HideAllChildrenExcept((int)data.type);
			
			if (data.type == IncomeListType.Vault) {
				root.GetComponentAtPath("Vault/CoinsText", out TextMeshProUGUI coinsText);
				root.GetComponentAtPath("Vault/TotalProfitText", out TextMeshProUGUI totalProfitText);
				coinsText.text = $"{Utils.FormatInt(data.vaultListData.coins)} <size=50>/</size> {Utils.FormatInt(data.vaultListData.storage)}";
				totalProfitText.text = $"Total Profit per turn: <color=#5DD900>{Utils.FormatInt(data.vaultListData.profitPerTurn)}</color>";
			} else if (data.type == IncomeListType.Top) {
				root.GetComponentAtPath("Top/TitleText", out TextMeshProUGUI titleText);
				titleText.text = $"{data.topListData.title}: <color=#5DD900>{Utils.FormatInt(data.topListData.profitPerTurn)}</color>";
			} else if (data.type == IncomeListType.Floor) {
				root.GetComponentAtPath("Floor/TitleText", out TextMeshProUGUI titleText);
				root.GetComponentAtPath("Floor/WaitingRoom/Text", out TextMeshProUGUI waitingRoomText);
				root.GetComponentAtPath("Floor/CallCenterRoom/Text", out TextMeshProUGUI callCenterRoomText);
				root.GetComponentAtPath("Floor/BreakRoom/Text", out TextMeshProUGUI breakRoomText);
				root.GetComponentAtPath("Floor/WaitingRoom/Level/LevelText", out TextMeshProUGUI waitingRoomLevelText);
				root.GetComponentAtPath("Floor/CallCenterRoom/Level/LevelText", out TextMeshProUGUI callCenterRoomLevelText);
				root.GetComponentAtPath("Floor/BreakRoom/Level/LevelText", out TextMeshProUGUI breakRoomLevelText);
				root.GetComponentAtPath("Floor/Line", out RectTransform line);
				root.GetComponentAtPath("Floor/BottomBackground", out RectTransform bottomBackground);
				titleText.text = $"Floor {data.florListData.level + 1}:  <color=#5DD900>{Utils.FormatInt(data.florListData.profitPerTurn)}</color>";
				waitingRoomText.text = $"Waiting Room: <color=#5DD900>{Utils.FormatInt(data.florListData.waitingRoom.CoinsIncome)}</color>";
				callCenterRoomText.text = $"Call Center Room: <color=#5DD900>{Utils.FormatInt(data.florListData.callCenterRoom.CoinsIncome)}</color>";
				breakRoomText.text = $"Break Room: <color=#5DD900>{Utils.FormatInt(data.florListData.breakRoom.CoinsIncome)}</color>";
				waitingRoomLevelText.text = data.florListData.waitingRoom.level.ToString();
				callCenterRoomLevelText.text = data.florListData.callCenterRoom.level.ToString();
				breakRoomLevelText.text = data.florListData.breakRoom.level.ToString();
				line.gameObject.SetActive(!data.florListData.isLast);
				bottomBackground.gameObject.SetActive(data.florListData.isLast);
			} else if (data.type == IncomeListType.Drivers) {
				root.GetComponentAtPath("Driver/Driver/MaskIcon/Icon", out Image driverImage);
				root.GetComponentAtPath("Driver/Driver/Text", out TextMeshProUGUI driverText);
				root.GetComponentAtPath("Driver/Background", out RectTransform background);
				root.GetComponentAtPath("Driver/BottomBackground", out RectTransform bottomBackground);
				driverImage.sprite = Resources.Load<Sprite>(data.driverListData.driver.design.spritePath);
				driverText.text = $"{data.driverListData.driver.design.name}: <color=#5DD900>{Utils.FormatInt(data.driverListData.driver.design.income)}</color>";
				background.gameObject.SetActive(!data.driverListData.isLast);
				bottomBackground.gameObject.SetActive(data.driverListData.isLast);
			}
		}
	}
	
	public class VaultListData {
		public int coins;
		public int storage;
		public int profitPerTurn;
	}
	
	public class TopListData {
		public string title;
		public int profitPerTurn;
	}
	
	public class FloorListData {
		public int level;
		public int profitPerTurn;
		public RoomData waitingRoom;
		public RoomData callCenterRoom;
		public RoomData breakRoom;
		public bool isLast;
	}
	
	public class DriverListData {
		public DriverData driver;
		public bool isLast;
	}
}


