###
 # @Descripttion: 
 # @Author: zhengjinhong
 # @Date: 2020-12-07 15:30:39
 # @LastEditors: zhengjinhong
 # @LastEditTime: 2020-12-09 15:56:19
 ###
rm -rf code/engines

mkdir -p code/engines/elog/ilog
cp -r  engines/elog/ilog/*  code/engines/elog/ilog
echo "copy elog ok..."

mkdir -p code/engines/etcp/inet
cp -r  engines/etcp/inet/*   code/engines/etcp/inet
echo "copy etcp ok..."

mkdir -p code/engines/etimer/itimer
cp -r  engines/etimer/itimer/*   code/engines/etimer/itimer
echo "copy etimer ok..."


mkdir -p code/engines/lib/Debug
cp -r  engines/lib/Debug/netcoreapp3.1/*.dll    code/engines/lib/Debug
cp -r  engines/lib/Debug/netcoreapp3.1/*.pdb    code/engines/lib/Debug

mkdir -p code/engines/lib/Release
cp -r  engines/lib/Release/netcoreapp3.1/*dll   code/engines/lib/Release
cp -r  engines/lib/Release/netcoreapp3.1/*pdb   code/engines/lib/Release
cp -r  code/temp/engines.csproj code/engines

mkdir -p code/bin/battleserver/netcoreapp3.1
mkdir -p code/bin/battleserver/netcoreapp3.1
cp -r  code/bin/battleserver/server_cfg.xml  code/bin/battleserver/netcoreapp3.1
cp -r  code/bin/battleserver/server_cfg.xml  code/bin/battleserver/netcoreapp3.1
echo "copy lib ok..."


