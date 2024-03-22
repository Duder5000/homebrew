# Function to convert arrow characters to corresponding key codes
function ConvertToKeyCode($arrow) {
    switch ($arrow) {
        "&#x2191;" { return 30 }
        "&#x2190;" { return 17 }
        "&#x2193;" { return 31 }
        "&#x2192;" { return 32 }
        default { return $null }
    }
}

# Function to generate XML document based on arrow characters
function GenerateXmlDocument($arrows, $filePath) {
    #$counter = 0
	$delay = 50
    $arrowArray = $arrows -split " "
			
	#$filePathArray = "C:\Users\Duder5000\Desktop\testArray.txt"
	#Set-Content -Path $filePathArray -Value $arrowArray	
	
	$xmlDoc = New-Object System.Xml.XmlDocument
	
	# Add XML declaration
	$xmlDeclaration = $xmlDoc.CreateXmlDeclaration("1.0", "utf-8", $null)
	$xmlDoc.AppendChild($xmlDeclaration)

	# Create the root element with namespaces
	$rootNode = $xmlDoc.CreateElement("Macro")
	$rootNode.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance")
	$rootNode.SetAttribute("xmlns:xsd", "http://www.w3.org/2001/XMLSchema")
	$xmlDoc.AppendChild($rootNode)

	# Create some child elements
	$macroName = "name1"
	$childNode1 = $xmlDoc.CreateElement("Name")
	$childNode1.InnerText = $macroName
	$rootNode.AppendChild($childNode1)

	$macroGuid = "47aba714-6bd7-4e42-921f-993337c218d9"
	$childNode2 = $xmlDoc.CreateElement("Guid")
	$childNode2.InnerText = $macroGuid
	$rootNode.AppendChild($childNode2)
	
	# Create a parent element
	$macroEventsNode = $xmlDoc.CreateElement("MacroEvents")
	$rootNode.AppendChild($macroEventsNode)

	# Create nested child elements within the parent
	$childNode3 = $xmlDoc.CreateElement("Child3")
	$childNode3.InnerText = "Value3"
	$macroEventsNode.AppendChild($childNode3)

	foreach ($arrow in $arrowArray) {
		$keyTemp = ConvertToKeyCode $arrow		
		#$counter += 1
	}
	
	# Save the XML document to a file
	$xmlDoc.Save("C:\Users\Duder5000\Desktop\file.xml")
}

# Usage example
$arrows = "&#x2191; &#x2193; &#x2192; &#x2190; &#x2191;"
$filePath = "C:\Users\Duder5000\Desktop\GeneratedMacro.xml"
GenerateXmlDocument $arrows $filePath