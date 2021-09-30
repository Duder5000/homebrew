<?php
	require('header.php');

	require('class-wraper-top.php');
	require('classes/fighter.php');	
	require('classes/ranger.php');	
	require('classes/rogue.php');	
	require('classes/sorcerer.php');	
	require('classes/warlock.php');	
	require('wraper-bottom.php');

	require('spell-wraper-top.php');
	require('spells/blight-touch.php');
	require('spells/spellscar-blade.php');
	require('spells/acid-arrows.php');
	require('wraper-bottom.php');

	require('races-wraper-top.php');
	require('races/barbarus-aarakocra.php');
	require('races/chiroptera.php');
	require('races/ratkin.php');
	require('wraper-bottom.php');

	require('feats-wraper-top.php');
	require('feats/arcane-shot-adept.php');
	require('wraper-bottom.php');

	require('warlock-invocations-wraper-top.php');
	require('warlock-invocations/eldritch-resolve.html');
	require('wraper-bottom.php');

	echo('</div>');
	
	require('sidebar-framework.php');
	require('sidebar-inv.php');
	require('wraper-bottom.php');
	echo('</div>');
	require('sidebar-framework.php');
	require('sidebar-key.php');
	require('wraper-bottom.php');
	echo('</div>');
	require('sidebar-img.php');

	require('footer.php');
?>