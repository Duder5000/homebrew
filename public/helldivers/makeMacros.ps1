function ConvertToKeyCode($arrow) {
    switch ($arrow) {
        "&#x2191;" { return 30 }
        "&#x2190;" { return 17 }
        "&#x2193;" { return 31 }
        "&#x2192;" { return 32 }
        default { return $null }
    }
}

function macroLoop($myArrow) { 
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
}

function GenerateXmlDocument($arrows, $filePath) {
	$delay = 50
    $arrowArray = $arrows -split " "
	
	$macroName = "500kg"
	$childNode1 = $xmlDoc.CreateElement("Name")
	$macroGuid = "47aba714-6bd7-4e42-921f-993337c218d9"
	$folderGuid = "00000000-0000-0000-0000-000000000000"
	
	$xmlDoc = New-Object System.Xml.XmlDocument
	
	$xmlDeclaration = $xmlDoc.CreateXmlDeclaration("1.0", "utf-8", $null)
	$xmlDoc.AppendChild($xmlDeclaration)

	$rootNode = $xmlDoc.CreateElement("Macro")
	$rootNode.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance")
	$rootNode.SetAttribute("xmlns:xsd", "http://www.w3.org/2001/XMLSchema")
	$xmlDoc.AppendChild($rootNode)
	
	$childNode1.InnerText = $macroName
	$rootNode.AppendChild($childNode1)

	$childNode2 = $xmlDoc.CreateElement("Guid")
	$childNode2.InnerText = $macroGuid
	$rootNode.AppendChild($childNode2)
	
	$macroEventsNode = $xmlDoc.CreateElement("MacroEvents")
	$rootNode.AppendChild($macroEventsNode)

###############################################################################################

	foreach ($arrow in $arrowArray) {
		macroLoop($arrow)
	}
	
	$macroIsFolder = $xmlDoc.CreateElement("IsFolder")
	$macroIsFolder.InnerText = "false"
	$rootNode.AppendChild($macroIsFolder)
	
	$macroFolderGuid = $xmlDoc.CreateElement("FolderGuid")
	$macroFolderGuid.InnerText = $folderGuid
	$rootNode.AppendChild($macroFolderGuid)
	
	$xmlDoc.Save("C:\Users\Duder5000\Desktop\file.xml")
}

# Test Values
$arrows = "&#x2191; &#x2193; &#x2192; &#x2190; &#x2191;"
$filePath = "C:\Users\Duder5000\Desktop\GeneratedMacro.xml"
GenerateXmlDocument $arrows $filePath