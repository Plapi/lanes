using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class AdCreatorTake2 : MonoBehaviour {

	[SerializeField] private Transform cameraTransform;
	[SerializeField] private VaultRoom vaultRoom;
	[SerializeField] private Room waitingRoom;
	[SerializeField] private Room breakRoom;
	[SerializeField] private Room callCenterRoom;
	[SerializeField] private ParkingRoom parkingRoom;
	[SerializeField] private CompanyController companyController;
	[SerializeField] private MeshRenderer roof;
	[SerializeField] private int[] moneyTablesTransitions;

	[Space]
	[SerializeField] private AudioSource audioSource;
	[SerializeField] private RectTransform transcriptRect;
	[SerializeField] private TextMeshProUGUI transcriptText;
	[SerializeField] private string[] transcriptTexts;
	[SerializeField] private float[] transcriptTimes;
	
	[Space]
	[SerializeField] private RectTransform installRect;
	[SerializeField] private RectTransform logoRect;
	
	private void Awake() {
		FindObjectsByType<CameraController>(FindObjectsSortMode.None)[0].gameObject.SetActive(false);
		PlayerPrefsManager.UserData.floors = new FloorData[] { new() };
		transcriptRect.gameObject.SetActive(false);
	}

	private IEnumerator VaultTable() {
		yield return new WaitForSeconds(0.1f);
		for (int i = 0; i < moneyTablesTransitions.Length; i++) {
			vaultRoom.UpdateTables(moneyTablesTransitions[i]);
			yield return new WaitForSeconds(0.1f);
		}
	}
	
	private IEnumerator Start() {
		yield return new WaitForSeconds(0.5f);
		roof.enabled = false;
		PlayerPrefsManager.UserData.parkingRoom.level = 1;
		parkingRoom.SetRoomGraphic();
		yield return new WaitForSeconds(2f);
		
		StartCoroutine(VaultTable());
		StartCoroutine(Transcripts());
		
		for (int i = 0; i < 9; i++) {
			vaultRoom.RoomData.level++;
			vaultRoom.SetRoomGraphic();
			yield return new WaitForSeconds(0.2f);
		}
		
		yield return new WaitForSeconds(0.3f);
		
		for (int i = 0; i < 9; i++) {
			waitingRoom.RoomData.level++;
			waitingRoom.SetRoomGraphic();
			breakRoom.RoomData.level++;
			breakRoom.SetRoomGraphic();
			yield return new WaitForSeconds(0.3f);
		}
		
		for (int i = 0; i < 9; i++) {
			callCenterRoom.RoomData.level++;
			callCenterRoom.SetRoomGraphic();
			parkingRoom.RoomData.level++;
			parkingRoom.SetRoomGraphic();
			parkingRoom.SetCar(PlayerPrefsManager.UserData.parkingRoom.parkingSlots[i]);
			yield return new WaitForSeconds(0.3f);
		}
		
		roof.enabled = true;
		for (int i = 0; i <= 7; i++) {
			PlayerPrefsManager.UserData.UpgradeFloor();
			companyController.UpgradeFloor(null);
			yield return new WaitForSeconds(0.2f);
		}
	}

	private IEnumerator Transcripts() {
		yield return new WaitForSeconds(0.2f);

		audioSource.Play();
		transcriptRect.gameObject.SetActive(true);
		
		for (int i = 0; i < transcriptTexts.Length; i++) {
			transcriptText.text = transcriptTexts[i];
			this.EndOfFrame(() => {
				HorizontalLayoutGroup horizontalLayoutGroup = transcriptRect.GetComponent<HorizontalLayoutGroup>();
				horizontalLayoutGroup.enabled = false;
				horizontalLayoutGroup.enabled = true;
			});
			yield return new WaitForSeconds(transcriptTimes[i]);
		}
		transcriptRect.gameObject.SetActive(false);
		
		installRect.gameObject.SetActive(true);
		installRect.SetAnchorPosY(-150f);
		installRect.DOAnchorPosY(80f, 0.3f).SetEase(Ease.OutExpo);
		
		yield return new WaitForSeconds(2f);
		
		logoRect.gameObject.SetActive(true);
		logoRect.SetAnchorPosY(300f);
		logoRect.DOAnchorPosY(-40f, 0.3f).SetEase(Ease.OutExpo);
	}

}
