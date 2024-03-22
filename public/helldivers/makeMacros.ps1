function ConvertToKeyCode($arrow) {
    switch ($arrow) {
        "&#x2191;" { return 30 }
        "&#x2190;" { return 17 }
        "&#x2193;" { return 31 }
        "&#x2192;" { return 32 }
		"29" { return 29 }
        default { return $null }
    }
}

function keys($myArrow, $keyState) { 
	$MacroEventSingleNode = $xmlDoc.CreateElement("MacroEvent")
	$macroEventsNode.AppendChild($MacroEventSingleNode)
	
	$MacroEventType = $xmlDoc.CreateElement("Type")
	$MacroEventType.InnerText = "1"
	$MacroEventSingleNode.AppendChild($MacroEventType)
	
	$MacroEventKey = $xmlDoc.CreateElement("KeyEvent")
	$MacroEventSingleNode.AppendChild($MacroEventKey)
	
	$keyCodeId = ConvertToKeyCode($myArrow)
	$MacroEventCode = $xmlDoc.CreateElement("Makecode")
	$MacroEventCode.InnerText = $keyCodeId
	$MacroEventKey.AppendChild($MacroEventCode)
	
	if($keyState -eq "u"){
		$MacroState = $xmlDoc.CreateElement("State")
		$MacroState.InnerText = "1"
		$MacroEventKey.AppendChild($MacroState)
	}
}

function GenerateXmlDocument($arrows, $macroName, $macroGuid, $pathBase) {
	$delay = 50
    $arrowArray = $arrows -split " "
	$folderGuid = "00000000-0000-0000-0000-000000000000"
		
	$xmlDoc = New-Object System.Xml.XmlDocument
	
	$xmlDeclaration = $xmlDoc.CreateXmlDeclaration("1.0", "utf-8", $null)
	$xmlDoc.AppendChild($xmlDeclaration)

	$rootNode = $xmlDoc.CreateElement("Macro")
	$rootNode.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance")
	$rootNode.SetAttribute("xmlns:xsd", "http://www.w3.org/2001/XMLSchema")
	$xmlDoc.AppendChild($rootNode)
	
	$childNode1 = $xmlDoc.CreateElement("Name")
	$childNode1.InnerText = $macroName
	$rootNode.AppendChild($childNode1)

	$childNode2 = $xmlDoc.CreateElement("Guid")
	$childNode2.InnerText = $macroGuid
	$rootNode.AppendChild($childNode2)
	
	$macroEventsNode = $xmlDoc.CreateElement("MacroEvents")
	$rootNode.AppendChild($macroEventsNode)

	keys "29" "d" #For Ctrl

	foreach ($arrow in $arrowArray) {
		keys $arrow "d"
		keys $arrow "u"
	}
	
	keys "29" "u" #For Ctrl
	
	$macroIsFolder = $xmlDoc.CreateElement("IsFolder")
	$macroIsFolder.InnerText = "false"
	$rootNode.AppendChild($macroIsFolder)
	
	$macroFolderGuid = $xmlDoc.CreateElement("FolderGuid")
	$macroFolderGuid.InnerText = $folderGuid
	$rootNode.AppendChild($macroFolderGuid)
	
	$xmlFileName = $pathBase +  $macroName + ".xml"
	$xmlDoc.Save($xmlFileName)
}

#####Test Values#####
$name = "def"
$arrows = "&#x2191; &#x2193; &#x2192; &#x2190; &#x2191;"
#$id = "47aba714-6bd7-4e42-921f-993337c218d9"
$id = "47aba714-0000-4e42-921f-993337c218d9"
$path = "C:\Users\Duder5000\Desktop\"

GenerateXmlDocument $arrows $name $id $path